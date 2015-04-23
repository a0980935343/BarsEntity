using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using Barsix.BarsEntity.BarsOptions;

using EnvDTE;

namespace Barsix.BarsEntity
{
    using BarsGenerators;
    using CodeGeneration;

    public partial class EntityOptionsWindow : Form
    {
        public EntityOptionsWindow()
        {
            InitializeComponent();
            Options = new EntityOptions();

            tbEntityName.Text = "NewEntity";
            tbMigrationVersion.Text = DateTime.Now.ToString("yyyy_MM_dd_00");

            cbvSelectionModel.SelectedIndex = 0;
            cbeBaseClass.SelectedIndex = 0;
        }

        public Project Project;

        public EntityOptions Options;

        public ListViewItem lviEntity;
        public ListViewItem lviMap;

        private void ComposeOptions()
        {
            Options.ClassName = tbEntityName.Text;
            Options.ClassFullName = Project.Name + ".Entities." + tbEntityName.Text;
            Options.TableName = tbMapTable.Text;
            Options.BaseClass = cbeBaseClass.Text;
            Options.IsDictionary = chDictionary.Checked;
            Options.Stateful = chStateful.Checked;
            Options.Signable = chSignable.Checked;
            Options.MigrationVersion = tbMigrationVersion.Text;
            Options.AuditLogMap = chmLogMap.Checked;

            foreach (ListViewItem item in lvFields.Items)
            {
                FieldOptions fopt = (FieldOptions)item.Tag;
                //if (Options.Stateful && fopt.FieldName == "State")
                //    continue;

                Options.Fields.Add(fopt);
            }

            if (chvTreeGrid.Checked && !Options.Fields.Any(x => x.ParentReference && x.TypeName == Options.ClassName))
            {
                Options.Fields.Add(new FieldOptions
                {
                    FieldName = "Parent",
                    TypeName = Options.ClassName,
                    ColumnName = "PARENT_ID",
                    ReferenceTable = Options.TableName,
                    DisplayName = "Родитель",
                    ParentReference = true,
                    ViewType = "easselectfield"
                });
            }

            if (Options.Stateful)
            {
                if (!Options.Fields.Any(x => x.FieldName == "State" && x.TypeName == "State"))
                {
                    Options.Fields.Add(new FieldOptions
                    {
                        FieldName = "State",
                        TypeName = "State",
                        ColumnName = "STATE_ID",
                        ReferenceTable = "EAS_STATE",
                        Index = Options.ClassName.CamelToSnake() + "__STATE",
                        DisplayName = "Статус",
                        ViewType = "easstatefield"
                    });
                }
                else
                {
                    Options.Fields.First(x => x.FieldName == "State" && x.TypeName == "State").ViewType = "easstatefield";
                }
            }
            if (Options.BaseClass == "NamedBaseEntity")
            {
                if (!Options.Fields.Any(x => x.FieldName == "Name" && x.TypeName == "string"))
                {
                    throw new Exception("Не задано строковое поле Name (требуется для NamedBaseEntity)");
                }
            }

            Options.AcceptFiles = Options.Fields.Any(x => x.TypeName == "FileInfo");

            if (!string.IsNullOrEmpty(tbcName.Text))
                Options.Controller = new ControllerOptions()
                {
                    Name = tbcName.Text,
                    Inline = chvInline.Checked
                };

            if (tbpPrefix.Text != "")
            {
                Options.Permission = new PermissionOptions()
                {
                    Prefix = tbpPrefix.Text,
                    NeedNamespace = chpNeedNamespace.Checked,
                    SimpleCRUDMap = chpSimpleCRUDMap.Checked
                };
            }

            Options.View = new ViewOptions()
            {
                Namespace = tbvNamespace.Text,
                Title = tbvEntityDisplayName.Text,
                EditingDisabled = chvEditing.Checked,
                SelectionModel = cbvSelectionModel.Text,
                DynamicFilter = chvDynamicFilter.Checked,
                TreeGrid = chvTreeGrid.Checked
            };
            Options.DisplayName = tbvEntityDisplayName.Text;

            if (tbnRoot.Text != "" && tbnName.Text != "")
            {
                Options.Navigation = new NavigationOptions()
                {
                    Root = tbnRoot.Text,
                    Name = tbnName.Text,
                    Anchor = tbnAnchor.Text,
                    MapPermission = chnMapPermissions.Checked
                };
            }

            InterceptorOptions dsicOpts = new InterceptorOptions();
            if (chdCreateBefore.Checked || Options.Stateful) dsicOpts.Actions.Add("BeforeCreate");
            if (chdCreateAfter.Checked) dsicOpts.Actions.Add("AfterCreate");

            if (chdUpdateBefore.Checked) dsicOpts.Actions.Add("BeforeUpdate");
            if (chdUpdateAfter.Checked) dsicOpts.Actions.Add("AfterUpdate");

            if (chdDeleteBefore.Checked) dsicOpts.Actions.Add("BeforeDelete");
            if (chdDeleteAfter.Checked) dsicOpts.Actions.Add("AfterDelete");

            if (dsicOpts.Actions.Any())
                Options.Interceptor = dsicOpts;

            DomainServiceOptions dsOpts = new DomainServiceOptions
            {
                Save = chdsSave.Checked,
                Update = chdsUpdate.Checked,
                Delete = chdsDelete.Checked,
                SaveInternal = chdsSaveInternal.Checked,
                UpdateInternal = chdsUpdateInternal.Checked,
                DeleteInternal = chdsDeleteInternal.Checked
            };
            if (dsOpts.Save || dsOpts.Delete || dsOpts.Update)
                Options.DomainService = dsOpts;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                ComposeOptions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (File.Exists(Path.Combine(Project.RootFolder(), "Migrations\\Version_{0}\\UpdateSchema.cs".F(Options.MigrationVersion))))
            {
                MessageBox.Show("Миграция с номером версии 'Version_{0}' уже существует! Измените версию.".F(Options.MigrationVersion), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ConfirmCreationParts confirmDialog = new ConfirmCreationParts();

            confirmDialog.chEntity.Checked = true;
            confirmDialog.chMap.Checked = !string.IsNullOrEmpty(Options.TableName);
            confirmDialog.chController.Checked = Options.Controller != null;
            confirmDialog.chMigration.Checked = !string.IsNullOrEmpty(Options.TableName);
            confirmDialog.chView.Checked = !string.IsNullOrEmpty(Options.View.Namespace);

            confirmDialog.chDomainService.Checked = confirmDialog.chDomainService.Enabled = Options.DomainService != null;
            confirmDialog.chDomainServiceInterceptor.Checked = confirmDialog.chDomainServiceInterceptor.Enabled = Options.Interceptor != null;

            confirmDialog.chDynamicFilter.Enabled = confirmDialog.chDynamicFilter.Checked = Options.Fields.Any(x => x.DynamicFilter);

            confirmDialog.chSignableEntitiesManifest.Checked = confirmDialog.chSignableEntitiesManifest.Enabled = Options.Signable;
            confirmDialog.chStatefulEntitiesManifest.Checked = confirmDialog.chStatefulEntitiesManifest.Enabled = Options.Stateful;
            
            confirmDialog.chAuditLogMap.Checked = Options.AuditLogMap;

            if (confirmDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IList<IBarsGenerator> generators = new List<IBarsGenerator>();

                if (confirmDialog.chEntity.Checked)
                    generators.Add(new EntityGenerator());

                if (confirmDialog.chMap.Checked)
                    generators.Add(new MapGenerator());

                if (confirmDialog.chController.Checked)
                    generators.Add(new ControllerGenerator());

                if (confirmDialog.chView.Checked)
                    generators.Add(new ViewGenerator());

                if (confirmDialog.chMigration.Checked)
                    generators.Add(new MigrationGenerator());
                
                if (Options.Navigation != null)
                    generators.Add(new NavigationGenerator());

                if (Options.Permission != null)
                    generators.Add(new PermissionGenerator());

                if (confirmDialog.chDomainServiceInterceptor.Checked)
                    generators.Add(new InterceptorGenerator());

                if (confirmDialog.chDomainService.Checked)
                    generators.Add(new DomainServiceGenerator());

                if (confirmDialog.chDynamicFilter.Checked)
                    generators.Add(new DynamicFilterGenerator());

                if (confirmDialog.chAuditLogMap.Checked)
                {
                    generators.Add(new AuditLogMapGenerator());
                    generators.Add(new AuditLogMapProviderGenerator());
                }

                if (confirmDialog.chStatefulEntitiesManifest.Checked)
                    generators.Add(new StatefulEntitiesManifestGenerator());

                if (confirmDialog.chSignableEntitiesManifest.Checked)
                    generators.Add(new SignableEntitiesManifestGenerator());
                
                DontForgetThis remainOps = GenerateEntity(generators);
                if (remainOps != null)
                {
                    remainOps.ShowDialog();
                }

                Close();
            }
        }

        private DontForgetThis GenerateEntity(IEnumerable<IBarsGenerator> generators)
        {
            StringBuilder remaining = new StringBuilder();
            GeneratedFragments dontForgetLines = new GeneratedFragments();

            foreach (var generator in generators)
            {
                try
                {
                    generator.Generate(Project, Options, dontForgetLines);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message + "\n\n" + ex.StackTrace, generator.GetType().Name);
                }
            }

            if (dontForgetLines.Any())
            {
                DontForgetThis form = new DontForgetThis();
                form.richTextBox1.Text = string.Join(Environment.NewLine, dontForgetLines.ToList());
                return form;
            }
            return null;
        }

        private void lvFields_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                lviEntity = e.Item;
                FieldOptions fopt = (FieldOptions)lviEntity.Tag;
                tbeName.Text = fopt.FieldName;
                tbeType.Text = fopt.TypeName;
                tbeComment.Text = fopt.Comment;
                cheOwnerReference.Checked = fopt.OwnerReference;
                cheParentReference.Checked = fopt.ParentReference;
                cheNullable.Checked = fopt.Nullable;
                cheNullable.Enabled = !(fopt.OwnerReference || fopt.ParentReference);
                cheList.Checked = fopt.Collection;

                btnUpsertEntityField.Text = "Обновить";
            }
            else
            {
                //if (e.Item == null)
                {
                    lviEntity       = null;
                    tbeName.Text    = "";
                    tbeType.Text    = "";
                    tbeComment.Text = "";
                    cheOwnerReference.Checked  = false;
                    cheParentReference.Checked = false;
                    cheNullable.Enabled = true;
                    cheNullable.Checked = false;
                    cheList.Checked = false;

                    btnUpsertEntityField.Text = "Создать";
                }
            }
        }

        private void btnUpsertEntityField_Click(object sender, EventArgs e)
        {
            if (lviEntity != null)
            {
                FieldOptions fopt = (FieldOptions)lviEntity.Tag;
                fopt.FieldName = tbeName.Text;

                if (fopt.TypeName != cbeBaseClass.Text)
                {
                    fopt.TypeName = tbeType.Text;
                    fopt.Collection = cheList.Checked;

                    fopt.SetRelatedTypes();
                }
                
                fopt.TypeName = tbeType.Text;
                fopt.Comment = tbeComment.Text;
                if (string.IsNullOrEmpty(fopt.DisplayName))
                    fopt.DisplayName = tbeComment.Text;
                fopt.OwnerReference = cheOwnerReference.Checked;
                fopt.Collection = cheList.Checked;
                fopt.ParentReference = cheParentReference.Checked;
                fopt.Nullable = cheNullable.Checked;
                
                lviEntity.Text = fopt.FieldName;
                lviEntity.SubItems[1].Text = fopt.FullTypeName;
                lviEntity.Tag = fopt;
            }
            else
            {
                FieldOptions fopt = new FieldOptions();
                fopt.FieldName = tbeName.Text;
                fopt.TypeName = tbeType.Text;
                fopt.Collection = cheList.Checked;
                fopt.SetRelatedTypes();

                if (fopt.Collection)
                {
                    fopt.ColumnName = tbEntityName.Text.CamelToSnake() + "_ID";
                }
                else
                {
                    fopt.ColumnName = tbeName.Text.CamelToSnake() + (!fopt.IsBasicType() ? "_ID" : "");
                }

                if (fopt.IsReference())
                {
                    if (fopt.TypeName == "FileInfo")
                        fopt.ReferenceTable = "B4_FILE_INFO";
                    else
                    if (fopt.TypeName == "State")
                        fopt.ReferenceTable = "EAS_STATE";
                    else
                    {
                        var parts = Project.Name.Split('.');

                        fopt.ReferenceTable = parts[1].ToUpper() + "_" + tbeType.Text.CamelToSnake();
                    }
                    fopt.Index = tbEntityName.Text.CamelToSnake() + "__" + tbeName.Text.CamelToSnake();
                }

                fopt.DisplayName = fopt.Comment = tbeComment.Text;
                fopt.OwnerReference = cheOwnerReference.Checked;
                fopt.ParentReference = cheParentReference.Checked;
                fopt.Nullable = cheNullable.Checked;

                var lvi = lvFields.Items.Add(fopt.FieldName);
                lvi.SubItems.Add(fopt.TypeName);
                lvi.Tag = fopt;

                lvi = lvMap.Items.Add(fopt.FieldName);
                lvi.SubItems.Add(fopt.ColumnName);
                lvi.Tag = fopt;

                lvi = lvView.Items.Add(fopt.FieldName);
                lvi.SubItems.Add(fopt.ViewType + " / " + fopt.ViewColumnType);
                lvi.Tag = fopt;


                lviEntity = null;
                tbeName.Text = "";
                tbeType.Text = "";
                tbeComment.Text = "";
                cheOwnerReference.Checked = false;
                cheParentReference.Checked = false;
                cheNullable.Checked = false;
                cheNullable.Enabled = true;
                cheList.Checked = false;
            }
            UpdateListViews();
        }

        private void UpdateListViews()
        {
            foreach (ListViewItem lvi in lvView.Items)
            {
                FieldOptions fopt = (FieldOptions)lvi.Tag;
                lvi.Text = fopt.FieldName;
                lvi.SubItems[1].Text = fopt.ViewType + " / " + (fopt.ViewType == "easselectfield" ? "renderer: " + fopt.TextProperty : fopt.ViewColumnType);
            }

            foreach (ListViewItem lvi in lvFields.Items)
            {
                FieldOptions fopt = (FieldOptions)lvi.Tag;
                lvi.Text = fopt.FieldName;
                lvi.SubItems[1].Text = fopt.FullTypeName;
            }

            foreach (ListViewItem lvi in lvMap.Items)
            {
                FieldOptions fopt = (FieldOptions)lvi.Tag;
                lvi.Text = fopt.FieldName;
                lvi.SubItems[1].Text = fopt.ColumnName;
            }
        }

        private void tabPage2_Enter(object sender, EventArgs e)
        {
            string converted = tbEntityName.Text.CamelToSnake();
            
            if(converted != "" && tbMapTable.Text == "")
                tbMapTable.Text = "MOSKS_" + converted;
            Invalidate();
        }

        private void lvMap_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                lviMap = e.Item;
                FieldOptions fopt = (FieldOptions)lviMap.Tag;
                tbmColumn.Text = fopt.ColumnName;

                if (fopt.TypeName == "string")
                {
                    tbmLength.Text = fopt.Length.ToString();
                }
                tbmLength.Visible = fopt.TypeName == "string";

                if (fopt.IsReference())
                {
                    tbmForeignTable.Text = fopt.ReferenceTable;
                    tbmIndex.Text = fopt.Index;
                }
                tbmForeignTable.Visible = tbmIndex.Visible = fopt.IsReference();

                cheNullable.Checked = fopt.Nullable;
            }
        }

        private void btnUpsertMapField_Click(object sender, EventArgs e)
        {
            if (lviMap != null)
            {
                FieldOptions fopt = (FieldOptions)lviMap.Tag;
                fopt.ColumnName = tbmColumn.Text;
                if (fopt.IsReference())
                {
                    fopt.ReferenceTable = tbmForeignTable.Text;
                    fopt.Index = tbmIndex.Text;
                }
                
                if (fopt.TypeName == "string")
                {
                    try
                    {
                        fopt.Length = int.Parse(tbmLength.Text);
                    }
                    catch{ fopt.Length = 100; tbmLength.Text = ""; }
                }

                lviMap.SubItems[1].Text = fopt.ColumnName;
                lviMap.Tag = fopt;
            }
            UpdateListViews();
        }

        private void tabPage6_Enter(object sender, EventArgs e)
        {
            if (tbpPrefix.Text == "")
                tbpPrefix.Text = Project.Name.Substring(5) + "." + tbEntityName.Text;
        }

        private void tabPage3_Enter(object sender, EventArgs e)
        {
            if (tbcName.Text == "")
                tbcName.Text = tbEntityName.Text;
        }

        private void tabPage4_Enter(object sender, EventArgs e)
        {
            if (tbvNamespace.Text == "")
                tbvNamespace.Text = Project.Name.Substring(5) + "." + tbEntityName.Text;
        }

        private void tabPage5_Enter(object sender, EventArgs e)
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (lviView != null)
            {
                FieldOptions fopt = (FieldOptions)lviView.Tag;
                fopt.DisplayName = tbvDisplayName.Text;
                fopt.ViewType = tbvType.Text;
                fopt.DynamicFilter = chvDynamicField.Checked;
                fopt.GroupField = chvGroupField.Checked;

                if (fopt.IsReference())
                {
                    fopt.TextProperty = tbvTextProperty.Text;
                }

                lviView.SubItems[1].Text = fopt.ViewType + " / " + fopt.ViewColumnType;
                lviView.Tag = fopt;
            }
            UpdateListViews();
        }

        public ListViewItem lviView;

        private void lvView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                lviView = e.Item;
                FieldOptions fopt = (FieldOptions)lviView.Tag;
                tbvViewName.Text = fopt.FieldName;
                tbvType.Text = fopt.ViewType;
                tbvDisplayName.Text = fopt.DisplayName;
                chvDynamicField.Checked = fopt.DynamicFilter;
                chvGroupField.Checked = fopt.GroupField;

                tbvTextProperty.Text = fopt.TextProperty;
                tbvTextProperty.Visible = fopt.IsReference();
            }
        }

        private void chSignable_CheckedChanged(object sender, EventArgs e)
        {
            if (chSignable.Checked && !Project.HasReference("Bars.B4.Modules.DigitalSignature"))
            {
                MessageBox.Show("В проекте нет ссылки на B4.Modules.DigitalSignature!", "Зависимости", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void chmLogMap_CheckedChanged(object sender, EventArgs e)
        {
            if (chmLogMap.Checked && !Project.HasReference("Bars.B4.Modules.NHibernateChangeLog"))
            {
                MessageBox.Show("В проекте нет ссылки на B4.Modules.NHibernateChangeLog!", "Зависимости", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void chvDynamicFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (chvDynamicFilter.Checked && !Project.HasReference("Bars.MosKs.DynamicFilters"))
            {
                MessageBox.Show("В проекте нет ссылки на MosKs.DynamicFilters!", "Зависимости", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void chStateful_CheckedChanged(object sender, EventArgs e)
        {
            if (chStateful.Checked && !Project.HasReference("Bars.B4.Modules.States"))
            {
                MessageBox.Show("В проекте нет ссылки на B4.Modules.States!", "Зависимости", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void tbEntityName_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void cheOwnerReference_CheckedChanged(object sender, EventArgs e)
        {
            if (cheOwnerReference.Checked)
            {
                foreach (ListViewItem lvi in lvFields.Items)
                {
                    if (((FieldOptions)lvi.Tag).OwnerReference && ((FieldOptions)lvi.Tag).FieldName != tbeName.Text) 
                    {
                        MessageBox.Show("Поле {0} уже назначено ссылкой на владельца!".F(((FieldOptions)lvi.Tag).FieldName), "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        cheOwnerReference.Checked = false;
                        break;
                    }
                }
                if (cheOwnerReference.Checked)
                {
                    cheNullable.Checked = false;
                    cheNullable.Enabled = false;
                }
            }
        }

        private void cheParentReference_CheckedChanged(object sender, EventArgs e)
        {
            if (cheParentReference.Checked)
            {
                if (tbeType.Text != tbEntityName.Text)
                {
                    MessageBox.Show("Тип поля должен совпадать с именем класса!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    cheParentReference.Checked = false;
                }
                else
                foreach (ListViewItem lvi in lvFields.Items)
                {
                    if (((FieldOptions)lvi.Tag).ParentReference && ((FieldOptions)lvi.Tag).FieldName != tbeName.Text)
                    {
                        MessageBox.Show("Поле {0} уже назначено ссылкой на родителя!".F(((FieldOptions)lvi.Tag).FieldName), "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        cheParentReference.Checked = false;
                        break;
                    }

                    if (((FieldOptions)lvi.Tag).GroupField)
                    {
                        MessageBox.Show("Иерархия невозможна в таблице с группировкой!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        cheParentReference.Checked = false;
                        break;
                    }
                }
                if (cheParentReference.Checked)
                {
                    cheNullable.Checked = true;
                    cheNullable.Enabled = false;
                }
            }
        }

        private void cheList_CheckedChanged(object sender, EventArgs e)
        {
            if (cheList.Checked)
            {
                cheOwnerReference.Enabled = cheParentReference.Enabled = cheNullable.Enabled = 
                cheOwnerReference.Checked = cheParentReference.Checked = cheNullable.Checked = false;
            }
            else
            {
                cheOwnerReference.Enabled = cheParentReference.Enabled = cheNullable.Enabled = true;
            }
        }

        private void chvGroupField_CheckedChanged(object sender, EventArgs e)
        {
            if (chvGroupField.Checked)
            {
                foreach (ListViewItem lvi in lvFields.Items)
                {
                    if (((FieldOptions)lvi.Tag).GroupField && ((FieldOptions)lvi.Tag).FieldName != tbvViewName.Text)
                    {
                        MessageBox.Show("Поле {0} уже назначено группировкой таблицы!".F(((FieldOptions)lvi.Tag).FieldName), "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        chvGroupField.Checked = false;
                        break;
                    }

                    if (((FieldOptions)lvi.Tag).ParentReference)
                    {
                        MessageBox.Show("Группировка невозможна в иерархической таблице!".F(((FieldOptions)lvi.Tag).FieldName), "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        chvGroupField.Checked = false;
                        break;
                    }
                }
            }
        }
    }
}
