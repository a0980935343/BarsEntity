using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using FastColoredTextBoxNS;
using EnvDTE;

namespace Barsix.BarsEntity
{
    using BarsOptions;
    using BarsGenerators;
    using CodeGeneration;

    public partial class EntityOptionsWindow : Form
    {
        public EntityOptionsWindow()
        {
            InitializeComponent();
            
            tbEntityName.Text = "NewEntity";
            tbMigrationVersion.Text = DateTime.Now.ToString("yyyy_MM_dd_00");

            cbvSelectionModel.SelectedIndex = 0;
            cbeBaseClass.SelectedIndex = 0;

            _codeEditors.Add("Entity",  CreateEditor(tgEntity, "Entity"));
            _codeEditors.Add("Map",        CreateEditor(tgMap, "Map"));
            _codeEditors.Add("Controller", CreateEditor(tgController, "Controller"));
            _codeEditors.Add("View",       CreateEditor(tgView, "View"));
            _codeEditors.Add("DomainService", CreateEditor(tgDomainService, "DomainService"));
            _codeEditors.Add("Interceptor", CreateEditor(tgInterceptor, "Interceptor"));
            _codeEditors.Add("Migration",   CreateEditor(tgMigration, "Migration"));
            _codeEditors.Add("AuditLogMap", CreateEditor(tgLogMap, "AuditLogMap"));
            _codeEditors.Add("Filterable",  CreateEditor(tgFilterable, "Filterable"));
            _codeEditors.Add("SignableEntitiesManifest", CreateEditor(tgSignable, "SignableEntitiesManifest"));
            _codeEditors.Add("StatefulEntitiesManifest", CreateEditor(tgStateful, "StatefulEntitiesManifest"));

            foreach (var ctl in this.Controls.All().Where(x => x.GetType() == typeof(CheckBox) || x.GetType() == typeof(ComboBox)))
            {
                if (ctl is CheckBox)
                {
                    ((CheckBox)ctl).CheckedChanged += (s, ea) => UpdateEditors();
                }
                else
                    if (ctl is ComboBox)
                    {
                        ((ComboBox)ctl).SelectedIndexChanged += (s, ea) => UpdateEditors();
                    }
            }
        }

        private Dictionary<string, FastColoredTextBox> _codeEditors = new Dictionary<string, FastColoredTextBox>();

        private IEnumerable<string> _knownTypes = new List<string> { "BaseEntity" };

        private FastColoredTextBox CreateEditor(TabPage tab, string name)
        {
            var editor = new FastColoredTextBox();
            editor.Name = "fctb" + name;
            editor.Left = 0;
            editor.Top = 0;
            editor.Language = FastColoredTextBoxNS.Language.Custom;
            editor.Width = tab.ClientSize.Width;
            editor.Height = tab.ClientSize.Height;
            editor.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            editor.BorderStyle = BorderStyle.FixedSingle;
            editor.Font = new Font("Consolas", 10F);
            editor.ReadOnly = true;
            editor.CharHeight = 15;
            editor.CharWidth = 8;

            TextStyle BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
            TextStyle BoldStyle = new TextStyle(new SolidBrush(Color.FromArgb(43, 145, 175)), null, FontStyle.Regular);
            TextStyle GrayStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
            TextStyle MagentaStyle = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
            TextStyle GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
            TextStyle BrownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Regular);
            TextStyle MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);
            MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));

            editor.TextChanged += (s, e) =>
            {
                if (((FastColoredTextBox)s).Language == FastColoredTextBoxNS.Language.Custom)
                {
                    ((FastColoredTextBox)s).LeftBracket = '(';
                    ((FastColoredTextBox)s).RightBracket = ')';
                    ((FastColoredTextBox)s).LeftBracket2 = '\x0';
                    ((FastColoredTextBox)s).RightBracket2 = '\x0';
                    //clear style of changed range
                    e.ChangedRange.ClearStyle(BlueStyle, BoldStyle, GrayStyle, MagentaStyle, GreenStyle, BrownStyle);

                    //string highlighting
                    e.ChangedRange.SetStyle(BrownStyle, @"""""|@""""|''|@"".*?""|(?<!@)(?<range>"".*?[^\\]"")|'.*?[^\\]'");
                    //comment highlighting
                    e.ChangedRange.SetStyle(GreenStyle, @"//.*$", RegexOptions.Multiline);
                    e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
                    e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft);
                    //number highlighting
                    e.ChangedRange.SetStyle(MagentaStyle, @"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b");
                    //attribute highlighting
                    e.ChangedRange.SetStyle(GrayStyle, @"^\s*(?<range>\[.+?\])\s*$", RegexOptions.Multiline);
                    //class name highlighting
                    e.ChangedRange.SetStyle(BoldStyle, @"\b(" +string.Join("|", _knownTypes)+ @")\b");
                    //keyword highlighting
                    e.ChangedRange.SetStyle(BlueStyle, @"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|add|alias|ascending|descending|dynamic|from|get|global|group|into|join|let|orderby|partial|remove|select|set|value|var|where|yield)\b|#region\b|#endregion\b");

                    //clear folding markers
                    e.ChangedRange.ClearFoldingMarkers();

                    //set folding markers
                    e.ChangedRange.SetFoldingMarkers("{", "}");//allow to collapse brackets block
                    e.ChangedRange.SetFoldingMarkers(@"#region\b", @"#endregion\b");//allow to collapse #region blocks
                    e.ChangedRange.SetFoldingMarkers(@"/\*", @"\*/");//allow to collapse comment block
                }
            };

            tab.Controls.Add(editor);
            return editor;
        }

        private Project _project;
        public void SetProject(Project project)
        {
            _project = project;

            _manager = new GenerationManager(_project);
            _manager.AddGenerator(new EntityGenerator());
            _manager.AddGenerator(new MapGenerator());
            _manager.AddGenerator(new ControllerGenerator());
            _manager.AddGenerator(new ViewGenerator());
            _manager.AddGenerator(new MigrationGenerator());
            _manager.AddGenerator(new NavigationGenerator());
            _manager.AddGenerator(new PermissionGenerator());
            _manager.AddGenerator(new InterceptorGenerator());
            _manager.AddGenerator(new DomainServiceGenerator());
            _manager.AddGenerator(new FilterableGenerator());
            _manager.AddGenerator(new AuditLogMapGenerator());
            _manager.AddGenerator(new AuditLogMapProviderGenerator());
            _manager.AddGenerator(new StatefulEntitiesManifestGenerator());
            _manager.AddGenerator(new SignableEntitiesManifestGenerator());
        }

        public ListViewItem lviEntity;
        public ListViewItem lviMap;

        private GenerationManager _manager;
        
        private EntityOptions ComposeOptions()
        {
            EntityOptions  options = new EntityOptions();
            options.ClassName = tbEntityName.Text;
            options.ClassFullName = _project.Name + ".Entities." + tbEntityName.Text;
            options.TableName = tbMapTable.Text;
            options.BaseClass = cbeBaseClass.Text;
            options.IsDictionary = chDictionary.Checked;
            options.Stateful = chStateful.Checked;
            options.Signable = chSignable.Checked;
            options.MigrationVersion = tbMigrationVersion.Text;
            options.AuditLogMap = chmLogMap.Checked;

            foreach (ListViewItem item in lvFields.Items)
            {
                FieldOptions fopt = (FieldOptions)item.Tag;
                options.Fields.Add(fopt);
            }

            if (chvTreeGrid.Checked && !options.Fields.Any(x => x.ParentReference && x.TypeName == options.ClassName))
            {
                options.Fields.Add(new FieldOptions
                {
                    FieldName = "Parent",
                    TypeName = options.ClassName,
                    ColumnName = "PARENT_ID",
                    ReferenceTable = options.TableName,
                    DisplayName = "Родитель",
                    ParentReference = true,
                    ViewType = "easselectfield"
                });
            }

            if (options.Stateful)
            {
                if (!options.Fields.Any(x => x.FieldName == "State" && x.TypeName == "State"))
                {
                    options.Fields.Add(new FieldOptions
                    {
                        FieldName = "State",
                        TypeName = "State",
                        ColumnName = "STATE_ID",
                        ReferenceTable = "EAS_STATE",
                        Index = options.ClassName.CamelToSnake() + "__STATE",
                        DisplayName = "Статус",
                        ViewType = "easstatefield"
                    });
                }
                else
                {
                    options.Fields.First(x => x.FieldName == "State" && x.TypeName == "State").ViewType = "easstatefield";
                }
            }
            if (options.BaseClass == "NamedBaseEntity")
            {
                if (!options.Fields.Any(x => x.FieldName == "Name" && x.TypeName == "string"))
                {
                    throw new Exception("Не задано строковое поле Name (требуется для NamedBaseEntity)");
                }
            }

            options.AcceptFiles = options.Fields.Any(x => x.TypeName == "FileInfo");

            if (!string.IsNullOrEmpty(tbcName.Text))
                options.Controller = new ControllerOptions()
                {
                    Name = tbcName.Text                    
                };

            if (tbpPrefix.Text != "")
            {
                options.Permission = new PermissionOptions()
                {
                    Prefix = tbpPrefix.Text,
                    NeedNamespace = chpNeedNamespace.Checked,
                    SimpleCRUDMap = chpSimpleCRUDMap.Checked
                };
            }

            options.View = new ViewOptions()
            {
                Namespace = tbvNamespace.Text,
                Title = tbvEntityDisplayName.Text,
                EditingDisabled = chvEditing.Checked,
                SelectionModel = cbvSelectionModel.Text,
                DynamicFilter = chvDynamicFilter.Checked,
                TreeGrid = chvTreeGrid.Checked,
                Inline = chvInline.Checked
            };
            options.DisplayName = tbvEntityDisplayName.Text;

            if (tbnRoot.Text != "" && tbnName.Text != "")
            {
                options.Navigation = new NavigationOptions()
                {
                    Root = tbnRoot.Text,
                    Name = tbnName.Text,
                    Anchor = tbnAnchor.Text,
                    MapPermission = chnMapPermissions.Checked
                };
            }

            InterceptorOptions dsicOpts = new InterceptorOptions();
            if (chdCreateBefore.Checked || options.Stateful) dsicOpts.Actions.Add("BeforeCreate");
            if (chdCreateAfter.Checked) dsicOpts.Actions.Add("AfterCreate");

            if (chdUpdateBefore.Checked) dsicOpts.Actions.Add("BeforeUpdate");
            if (chdUpdateAfter.Checked) dsicOpts.Actions.Add("AfterUpdate");

            if (chdDeleteBefore.Checked) dsicOpts.Actions.Add("BeforeDelete");
            if (chdDeleteAfter.Checked) dsicOpts.Actions.Add("AfterDelete");

            if (dsicOpts.Actions.Any())
                options.Interceptor = dsicOpts;

            DomainServiceOptions dsOpts = new DomainServiceOptions
            {
                Save = chdsSave.Checked,
                Update = chdsUpdate.Checked,
                Delete = chdsDelete.Checked,
                SaveInternal = chdsSaveInternal.Checked,
                UpdateInternal = chdsUpdateInternal.Checked,
                DeleteInternal = chdsDeleteInternal.Checked
            };
            if (dsOpts.Save || dsOpts.Delete || dsOpts.Update || dsOpts.SaveInternal || dsOpts.DeleteInternal || dsOpts.UpdateInternal)
                options.DomainService = dsOpts;

            return options;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            EntityOptions options = null;
            try
            {
                options = ComposeOptions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (File.Exists(Path.Combine(_project.RootFolder(), "Migrations\\Version_{0}\\UpdateSchema.cs".F(options.MigrationVersion))))
            {
                MessageBox.Show("Миграция с номером версии 'Version_{0}' уже существует! Измените версию.".F(options.MigrationVersion), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ConfirmCreationParts confirmDialog = new ConfirmCreationParts();

            confirmDialog.chEntity.Checked = true;
            confirmDialog.chMap.Checked = !string.IsNullOrEmpty(options.TableName);
            confirmDialog.chController.Checked = options.Controller != null;
            confirmDialog.chMigration.Checked = !string.IsNullOrEmpty(options.TableName);
            confirmDialog.chView.Checked = !string.IsNullOrEmpty(options.View.Namespace);

            confirmDialog.chDomainService.Checked = confirmDialog.chDomainService.Enabled = options.DomainService != null;
            confirmDialog.chDomainServiceInterceptor.Checked = confirmDialog.chDomainServiceInterceptor.Enabled = options.Interceptor != null;

            confirmDialog.chDynamicFilter.Enabled = confirmDialog.chDynamicFilter.Checked = options.View.DynamicFilter && options.Fields.Any(x => x.DynamicFilter);

            confirmDialog.chSignableEntitiesManifest.Checked = confirmDialog.chSignableEntitiesManifest.Enabled = options.Signable;
            confirmDialog.chStatefulEntitiesManifest.Checked = confirmDialog.chStatefulEntitiesManifest.Enabled = options.Stateful;
            
            confirmDialog.chAuditLogMap.Checked = options.AuditLogMap;

            if (confirmDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<Type> generatorTypes = new List<Type>();

                if (confirmDialog.chEntity.Checked)
                    generatorTypes.Add(typeof(EntityGenerator));

                if (confirmDialog.chMap.Checked)
                    generatorTypes.Add(typeof(MapGenerator));

                if (confirmDialog.chController.Checked)
                    generatorTypes.Add(typeof(ControllerGenerator));

                if (confirmDialog.chView.Checked)
                    generatorTypes.Add(typeof(ViewGenerator));

                if (confirmDialog.chMigration.Checked)
                    generatorTypes.Add(typeof(MigrationGenerator));

                if (options.Navigation != null)
                    generatorTypes.Add(typeof(NavigationGenerator));

                if (options.Permission != null)
                    generatorTypes.Add(typeof(PermissionGenerator));

                if (confirmDialog.chDomainServiceInterceptor.Checked)
                    generatorTypes.Add(typeof(InterceptorGenerator));

                if (confirmDialog.chDomainService.Checked)
                    generatorTypes.Add(typeof(DomainServiceGenerator));

                if (confirmDialog.chDynamicFilter.Checked)
                    generatorTypes.Add(typeof(FilterableGenerator));

                if (confirmDialog.chAuditLogMap.Checked)
                {
                    generatorTypes.Add(typeof(AuditLogMapGenerator));
                    generatorTypes.Add(typeof(AuditLogMapProviderGenerator));
                }

                if (confirmDialog.chStatefulEntitiesManifest.Checked)
                    generatorTypes.Add(typeof(StatefulEntitiesManifestGenerator));

                if (confirmDialog.chSignableEntitiesManifest.Checked)
                    generatorTypes.Add(typeof(SignableEntitiesManifestGenerator));
                
                _manager.AddToProject(options, generatorTypes);
                if (_manager.Fragments.Any())
                {
                    DontForgetThis form = new DontForgetThis();
                    form.richTextBox1.Text = string.Join(Environment.NewLine, _manager.Fragments.ToList());
                    form.ShowDialog();
                }

                Close();
            }
        }

        private void lvFields_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                lviEntity = e.Item;
                FieldOptions fopt = (FieldOptions)e.Item.Tag;
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

        private void btnUpsertEntityField_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbeName.Text) || string.IsNullOrWhiteSpace(tbeType.Text))
            {
                MessageBox.Show("Укажите тип и название свойства");
                return;
            }

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
                        var parts = _project.Name.Split('.');

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

            UpdateEditors();
        }

        private void UpdateEditors()
        {
            if (_project == null)
                return;
            var options = ComposeOptions();

            _manager.Generate(options);
                        
            _codeEditors["View"].Language = (options.Controller != null && options.View.Inline) ? FastColoredTextBoxNS.Language.Custom : FastColoredTextBoxNS.Language.JS;

            List<string> types = new List<string> { "BaseEntity", options.ClassName };
            

            foreach (var gen in _manager.Generators)
            {
                string name = gen.GetType().Name.Substring(0, gen.GetType().Name.Length - "Generator".Length);
                if (!_codeEditors.ContainsKey(name))
                    continue;


                if (_manager.Files.Any(x => x.Key == gen.GetType() && x.Value != null))
                {
                    _knownTypes = gen.KnownTypes;
                    _codeEditors[name].Text = string.Join(Environment.NewLine, _manager.Files.First(x => x.Key == gen.GetType()).Value.Body);
                }
                else
                    _codeEditors[name].Text = "";
            }
        }

        private void tabPage2_Enter(object sender, EventArgs e)
        {
            string converted = tbEntityName.Text.CamelToSnake();

            if (converted != "" && tbMapTable.Text == "")
            {
                var parts = _project.Name.Split('.');
                tbMapTable.Text = parts[1].ToUpper() + "_" + converted;
            }
            UpdateEditors();
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
                tbpPrefix.Text = _project.Name.Substring(5) + "." + tbEntityName.Text;
            UpdateEditors();
        }

        private void tabPage3_Enter(object sender, EventArgs e)
        {
            if (tbcName.Text == "")
                tbcName.Text = tbEntityName.Text;
            UpdateEditors();
        }

        private void tabPage4_Enter(object sender, EventArgs e)
        {
            if (tbvNamespace.Text == "")
                tbvNamespace.Text = _project.Name.Substring(5) + "." + tbEntityName.Text;
            UpdateEditors();
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
            if (chSignable.Checked && !_project.HasReference("Bars.B4.Modules.DigitalSignature"))
            {
                MessageBox.Show("В проекте нет ссылки на B4.Modules.DigitalSignature!", "Зависимости", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            //UpdateEditors();
        }

        private void chmLogMap_CheckedChanged(object sender, EventArgs e)
        {
            if (chmLogMap.Checked && !_project.HasReference("Bars.B4.Modules.NHibernateChangeLog"))
            {
                MessageBox.Show("В проекте нет ссылки на B4.Modules.NHibernateChangeLog!", "Зависимости", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            //UpdateEditors();
        }

        private void chvDynamicFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (chvDynamicFilter.Checked && !_project.HasReference("Bars.MosKs.DynamicFilters"))
            {
                MessageBox.Show("В проекте нет ссылки на MosKs.DynamicFilters!", "Зависимости", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            if (chvDynamicFilter.Checked && !_project.HasReference("Bars.B4.Modules.ReportPanel"))
            {
                MessageBox.Show("В проекте нет ссылки на B4.Modules.ReportPanel!", "Зависимости", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            //UpdateEditors();
        }

        private void chStateful_CheckedChanged(object sender, EventArgs e)
        {
            if (chStateful.Checked && !_project.HasReference("Bars.B4.Modules.States"))
            {
                MessageBox.Show("В проекте нет ссылки на B4.Modules.States!", "Зависимости", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            //UpdateEditors();
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
            //UpdateEditors();
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
            //UpdateEditors();
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
            //UpdateEditors();
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
            //UpdateEditors();
        }

        private void lvFields_KeyUp(object sender, KeyEventArgs e)
        {
            if (lvFields.SelectedItems.Count > 0)
            {
                var fopt = lvFields.SelectedItems[0].Tag;

                if (e.KeyCode == Keys.Delete)
                {
                    foreach (ListViewItem item in lvMap.Items)
                    {
                        if (item.Tag == fopt)
                        {
                            lvMap.Items.Remove(item);
                            break;
                        }
                    }

                    foreach (ListViewItem item in lvView.Items)
                    {
                        if (item.Tag == fopt)
                        {
                            lvView.Items.Remove(item);
                            break;
                        }
                    }

                    foreach (ListViewItem item in lvFields.Items)
                    {
                        if (item.Tag == fopt)
                        {
                            lvFields.Items.Remove(item);
                            break;
                        }
                    }
                    UpdateListViews();
                }
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if (lvFields.SelectedItems.Count > 0 && lvFields.SelectedItems[0].Index > 0)
            {
                var fopt = lvFields.SelectedItems[0].Tag;
                lvFields.Items[lvFields.SelectedItems[0].Index].Tag = lvFields.Items[lvFields.SelectedItems[0].Index - 1].Tag;
                lvFields.Items[lvFields.SelectedItems[0].Index - 1].Tag = fopt;
                UpdateListViews();
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (lvFields.SelectedItems.Count > 0 && lvFields.SelectedItems[0].Index < lvFields.Items.Count-1)
            {
                var fopt = lvFields.SelectedItems[0].Tag;
                lvFields.Items[lvFields.SelectedItems[0].Index].Tag = lvFields.Items[lvFields.SelectedItems[0].Index + 1].Tag;
                lvFields.Items[lvFields.SelectedItems[0].Index + 1].Tag = fopt;
                UpdateListViews();
            }
        }
    }

    public static class Classs
    {
        public static IEnumerable<Control> All(this Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                foreach (Control grandChild in control.Controls.All())
                    yield return grandChild;

                yield return control;
            }
        }
    }

}
