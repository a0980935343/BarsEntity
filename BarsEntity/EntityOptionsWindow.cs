using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using FastColoredTextBoxNS;
using EnvDTE;

namespace Barsix.BarsEntity
{
    using Windows;
    using BarsOptions;
    using BarsGenerators;
    using CodeGeneration.CSharp;

    public partial class EntityOptionsWindow : Form
    {
        public EntityOptionsWindow()
        {
            InitializeComponent();

            tbEntityName.Text = tbcName.Text = "NewEntity";
            tbTableName.Text = tbEntityName.Text.CamelToSnake();

            tbMigrationVersion.Text = DateTime.Now.ToString("yyyy_MM_dd_00");

            cbvSelectionModel.SelectedIndex = 0;
            cbeBaseClass.SelectedIndex = 0;
            cbvViewType.SelectedIndex = 0;

            CreateEditor(tgEntity, "Entity");
            CreateEditor(tgMap, "Map");
            CreateEditor(tgController, "Controller");
            CreateEditor(tgView, "View");
            CreateEditor(tgDomainService, "DomainService");
            CreateEditor(tgInterceptor, "Interceptor");
            CreateEditor(tgMigration, "Migration");
            CreateEditor(tgLogMap, "AuditLogMap");
            CreateEditor(tgFilterable, "Filterable");
            CreateEditor(tgSignable, "SignableEntitiesManifest");
            CreateEditor(tgStateful, "StatefulEntitiesManifest");

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

        private void CreateEditor(TabPage tab, string name)
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
            editor.Tag = new List<string>();

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
                    e.ChangedRange.SetStyle(BoldStyle, @"\b(" +string.Join("|", ((IEnumerable<string>)((FastColoredTextBox)s).Tag) )+ @")\b");
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

            editor.KeyUp += (s, e) => { if (e.KeyCode == Keys.F5) UpdateEditors(); };

            tab.Controls.Add(editor);
            _codeEditors.Add(name, editor);
        }

        private Project _project;
        public void SetProject(Project project)
        {
            Text = "Создание Bars-объекта ({0})".F(project.Name);

            _project = project;

            tbTableName.Text = EntityHelper.TableNameByEntityName(tbEntityName.Text, _project.DefaultNamespace());

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
        
        private GenerationManager _manager;
        
        private EntityOptions ComposeOptions()
        {
            EntityOptions  options = new EntityOptions();
            options.ClassName = tbEntityName.Text;
            options.ClassFullName = _project.DefaultNamespace() + ".Entities." + tbEntityName.Text;
            options.TableName = tbTableName.Text;
            options.BaseClass = cbeBaseClass.Text;
            options.IsDictionary = chDictionary.Checked;
            options.Stateful = chStateful.Checked;
            options.Signable = chSignable.Checked;
            options.MigrationVersion = tbMigrationVersion.Text;
            options.AuditLogMap = chmLogMap.Checked;
            options.Subfolder = tbSubfolder.Text;

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
                    Name = tbcName.Text,
                    List = chcList.Checked,
                    Get  = chcGet.Checked,
                    Update = chcUpdate.Checked,
                    Delete = chcDelete.Checked,
                    Create = chcCreate.Checked
                };

            if (tbpPrefix.Text != "")
            {
                options.Permission = new PermissionOptions()
                {
                    Prefix = tbpPrefix.Text,
                    SimpleCRUDMap = chpSimpleCRUDMap.Checked
                };
            }

            options.View = new ViewOptions()
            {
                Namespace = tbvNamespace.Text,
                Title = tbvEntityDisplayName.Text,
                EditingDisabled = chvEditingDisabled.Checked,
                SelectionModel = cbvSelectionModel.Text,
                DynamicFilter = chvDynamicFilter.Checked,
                TreeGrid = chvTreeGrid.Checked,
                Type = (ViewType)(cbvViewType.SelectedIndex + 1)
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

        /// <summary>
        /// Проверить настройки сущности и предложить дополнить/устранить возможные нестыковки
        /// </summary>
        private bool AnalyzeOptions(EntityOptions options)
        {
            // для View нет ссылки на основную сущность
            if (options.ClassName.EndsWith("View"))
            {
                var viewEntity = options.ClassName.Substring(0, options.ClassName.Length - 4);

                if (options.Fields.FirstOrDefault(f => (f.FieldName == f.TypeName) && (f.FieldName == viewEntity)) == null &&
                    System.Windows.Forms.MessageBox.Show("В представлении нет ссылки на основную сущность (" + viewEntity + "). Добавить?",
                        "Настройка свойств", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    FieldOptions fopt = new FieldOptions();
                    fopt.FieldName = viewEntity;
                    fopt.TypeName = viewEntity;
                    fopt.Collection = false;
                    fopt.Enum = false;
                    fopt.SetRelatedTypes();

                    fopt.ColumnName = viewEntity.CamelToSnake() + "_ID";

                    var parts = _project.DefaultNamespace().Split('.');
                    fopt.ReferenceTable = (parts.Count() > 1 ? parts[1].ToUpper() + "_" : "") + viewEntity.CamelToSnake();

                    fopt.DisplayName = "";
                    fopt.OwnerReference = false;
                    fopt.ParentReference = false;
                    fopt.Nullable = false;

                    var lvi = lvFields.Items.Insert(0, fopt.FieldName);
                    lvi.SubItems.Add(fopt.TypeName);
                    lvi.Tag = fopt;

                    lvi = lvMap.Items.Add(fopt.FieldName);
                    lvi.SubItems.Add(fopt.ColumnName);
                    lvi.Tag = fopt;

                    lvi = lvView.Items.Add(fopt.FieldName);
                    lvi.SubItems.Add(fopt.ViewType + " / " + fopt.ViewColumnType);
                    lvi.SubItems.Add(fopt.DisplayName);
                    lvi.Tag = fopt;

                    return true;
                }
            }

            return false;
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
                MessageBox(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (AnalyzeOptions(options))
            {
                options = ComposeOptions();
                UpdateListViews();
            }

            if (File.Exists(Path.Combine(_project.RootFolder(), "Migrations\\Version_{0}\\UpdateSchema.cs".F(options.MigrationVersion))))
            {
                MessageBox("Миграция с номером версии 'Version_{0}' уже существует! Измените версию.".F(options.MigrationVersion), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ConfirmCreationParts confirmDialog = new ConfirmCreationParts();

            var isView = options.ClassName.EndsWith("View");

            confirmDialog.chEntity.Checked = true;
            confirmDialog.chMap.Checked = !string.IsNullOrEmpty(options.TableName);
            confirmDialog.chController.Checked = !isView && options.Controller != null;
            confirmDialog.chMigration.Checked = !string.IsNullOrEmpty(options.TableName);
            confirmDialog.chView.Checked = !isView && !string.IsNullOrEmpty(options.View.Namespace);

            confirmDialog.chDomainService.Checked = confirmDialog.chDomainService.Enabled = !isView && options.DomainService != null;
            confirmDialog.chDomainServiceInterceptor.Checked = confirmDialog.chDomainServiceInterceptor.Enabled = !isView && options.Interceptor != null;

            confirmDialog.chDynamicFilter.Enabled = confirmDialog.chDynamicFilter.Checked = !isView && options.View.DynamicFilter && options.Fields.Any(x => x.DynamicFilter);

            confirmDialog.chSignableEntitiesManifest.Checked = confirmDialog.chSignableEntitiesManifest.Enabled = !isView && options.Signable;
            confirmDialog.chStatefulEntitiesManifest.Checked = confirmDialog.chStatefulEntitiesManifest.Enabled = !isView && options.Stateful;

            confirmDialog.chAuditLogMap.Checked = !isView && options.AuditLogMap;

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

                if (options.Navigation != null && !options.Fields.Any(x => x.OwnerReference) && confirmDialog.chNavigation.Checked)
                    generatorTypes.Add(typeof(NavigationGenerator));

                if (options.Permission != null && confirmDialog.chPermission.Checked)
                    generatorTypes.Add(typeof(PermissionGenerator));

                if (confirmDialog.chDomainServiceInterceptor.Checked)
                    generatorTypes.Add(typeof(InterceptorGenerator));

                if (confirmDialog.chDomainService.Checked)
                    generatorTypes.Add(typeof(DomainServiceGenerator));

                if (confirmDialog.chDynamicFilter.Checked)
                    generatorTypes.Add(typeof(FilterableGenerator));

                if (confirmDialog.chAuditLogMap.Checked)
                    generatorTypes.Add(typeof(AuditLogMapGenerator));

                if (confirmDialog.chAuditLogMap.Checked)
                    generatorTypes.Add(typeof(AuditLogMapProviderGenerator));

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
                FieldOptions fopt = (FieldOptions)e.Item.Tag;
                tbeName.Text = fopt.FieldName;
                tbeType.Text = fopt.TypeName;
                tbeComment.Text = fopt.Comment;
                cheOwnerReference.Checked = fopt.OwnerReference;
                cheParentReference.Checked = fopt.ParentReference;
                cheNullable.Checked = fopt.Nullable;
                cheNullable.Enabled = !(fopt.OwnerReference || fopt.ParentReference);
                cheList.Checked = fopt.Collection;
                cheEnum.Checked = fopt.Enum;
                btnUpsertEntityField.Text = "Обновить";
            }
            else
            {
                tbeName.Text    = "";
                tbeType.Text    = "";
                tbeComment.Text = "";
                cheOwnerReference.Checked  = false;
                cheParentReference.Checked = false;
                cheNullable.Enabled = true;
                cheNullable.Checked = false;
                cheList.Checked = false;
                cheEnum.Checked = false;
                btnUpsertEntityField.Text = "Создать";
            }
        }

        private void btnUpsertEntityField_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbeName.Text) || string.IsNullOrWhiteSpace(tbeType.Text))
            {
                MessageBox("Укажите тип и название свойства", "Ошибка");
                return;
            }

            if (lvFields.SelectedItems.Count > 0)
            {
                var lviEntity = lvFields.SelectedItems[0];
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
                fopt.Enum = cheEnum.Checked;
                
                lviEntity.Text = fopt.FieldName;
                lviEntity.SubItems[1].Text = fopt.FullTypeName;
                lviEntity.Tag = fopt;

                if (fopt.ParentReference || fopt.OwnerReference || chvTreeGrid.Checked)
                {
                    chcList.Checked = true;
                    chcList.Enabled = false;
                }
                else
                {
                    chcList.Enabled = true;
                }
            }
            else
            {
                foreach (ListViewItem item in lvFields.Items)
                {
                    if (((FieldOptions)item.Tag).FieldName == tbeName.Text)
                    {
                        MessageBox("Свойство {0} уже существует".F(tbeName.Text), "Ошибка");
                        return;
                    }
                }

                FieldOptions fopt = new FieldOptions();
                fopt.FieldName = tbeName.Text;
                fopt.TypeName = tbeType.Text;
                fopt.Collection = cheList.Checked;
                fopt.Enum = cheEnum.Checked;
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
                        var parts = _project.DefaultNamespace().Split('.');

                        fopt.ReferenceTable = parts[parts.Count() > 1 ? 1 : 0].ToUpper() + "_" + tbeType.Text.CamelToSnake();
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
                lvi.SubItems.Add(fopt.DisplayName);
                lvi.Tag = fopt;
                
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
                lvi.SubItems[2].Text = fopt.DisplayName;
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
            if (_project == null || _preventInterfaceActions)
                return;
            try
            {
                var options = ComposeOptions();
                
                _manager.Generate(options);

                _codeEditors["View"].Language = (options.Controller != null && options.View.Type == ViewType.ViewModel) ? FastColoredTextBoxNS.Language.Custom : FastColoredTextBoxNS.Language.JS;

                List<string> types = new List<string> { "BaseEntity", options.ClassName };


                foreach (var gen in _manager.Generators)
                {
                    string name = gen.GetType().Name.Substring(0, gen.GetType().Name.Length - "Generator".Length);
                    if (!_codeEditors.ContainsKey(name))
                        continue;


                    if (_manager.Files.Any(x => x.Key == gen.GetType() && x.Value != null))
                    {
                        _codeEditors[name].Tag = gen.KnownTypes;
                        string filesBody = "";
                        foreach (var file in _manager.Files.First(x => x.Key == gen.GetType()).Value)
                        {
                            filesBody = filesBody + string.Join(Environment.NewLine, file.Body);
                            if (file != _manager.Files.First(x => x.Key == gen.GetType()).Value.Last())
                                filesBody = filesBody + Environment.NewLine + "//------------------------------------------------------------------------------------" + Environment.NewLine;
                        }
                        _codeEditors[name].Text = filesBody;
                    }
                    else
                        _codeEditors[name].Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox(ex.Message, "Ошибка");
                return;
            }
        }

        private void tabPage2_Enter(object sender, EventArgs e)
        {
            tbTableName.Text = EntityHelper.TableNameByEntityName(tbEntityName.Text, _project.DefaultNamespace());
            UpdateEditors();
        }

        private void lvMap_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                FieldOptions fopt = (FieldOptions)e.Item.Tag;
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
            else
            {
                tbmColumn.Text = tbmIndex.Text = tbmLength.Text = tbmName.Text = tbmForeignTable.Text = "";
            }
            btnUpsertMapField.Enabled = e.IsSelected;
        }

        private void btnUpsertMapField_Click(object sender, EventArgs e)
        {
            if (lvMap.SelectedItems.Count > 0)
            {
                var lviMap = lvMap.SelectedItems[0];

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
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (lvView.SelectedItems.Count > 0)
            {
                var lviView = lvView.SelectedItems[0];

                FieldOptions fopt = (FieldOptions)lviView.Tag;
                fopt.DisplayName = tbvDisplayName.Text;
                fopt.ViewType = tbvType.Text;
                fopt.DynamicFilter = chvDynamicField.Checked;
                fopt.GroupField = chvGroupField.Checked;

                if (fopt.IsReference())
                {
                    fopt.TextProperty = cbvTextProperty.SelectedItem != null ? ((StringComboItem)cbvTextProperty.SelectedItem).Value : cbvTextProperty.Text;
                }

                lviView.SubItems[1].Text = fopt.ViewType + " / " + fopt.ViewColumnType;
                lviView.Tag = fopt;
            }
            UpdateListViews();
        }

        private class StringComboItem
        {
            public string Value { get; set; }
            public string Display { get; set; }
        }

        private void lvView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                FieldOptions fopt = (FieldOptions)e.Item.Tag;
                tbvViewName.Text = fopt.FieldName;
                tbvType.Text = fopt.ViewType;
                tbvDisplayName.Text = fopt.DisplayName;
                chvDynamicField.Checked = fopt.DynamicFilter;
                chvGroupField.Checked = fopt.GroupField;

                cbvTextProperty.Text = fopt.TextProperty;
                cbvTextProperty.Visible = fopt.IsReference();

                var classes = _manager.ClassExists(fopt.TypeName).Values.FirstOrDefault();
                if (classes != null)
                {
                    cbvTextProperty.DisplayMember = "Display";
                    cbvTextProperty.ValueMember   = "Value";
                    var propList = new List<StringComboItem>();
                    if (classes.BaseClass == "NamedBaseEntity")
                        propList.Add(new StringComboItem { Value = "Name", Display = "Name  :  string" });

                    propList.AddRange(classes.Fields.Where(x => x.IsBasicType() && !x.Enum).Select(x => new StringComboItem { Value = x.FieldName, Display = x.FieldName + "  :  " + x.FullTypeName }));

                    cbvTextProperty.DataSource = propList.ToArray();
                }
            }
            else
            {
                cbvTextProperty.Text = tbvDisplayName.Text = tbvType.Text = tbvViewName.Text = "";
                chvDynamicField.Checked = chvGroupField.Checked = false;
            }
            btnUpsertViewField.Enabled = e.IsSelected;
        }

        private void chSignable_CheckedChanged(object sender, EventArgs e)
        {
            CheckReference(chSignable, "Bars.B4.Modules.DigitalSignature");
        }

        private void chmLogMap_CheckedChanged(object sender, EventArgs e)
        {
            CheckReference(chmLogMap, "Bars.B4.Modules.NHibernateChangeLog");
        }

        private void chvDynamicFilter_CheckedChanged(object sender, EventArgs e)
        {
            CheckReference(chvDynamicFilter, "Bars.MosKs.DynamicFilters");
            CheckReference(chvDynamicFilter, "Bars.B4.Modules.ReportPanel");
        }

        private void chStateful_CheckedChanged(object sender, EventArgs e)
        {
            CheckReference(chStateful, "Bars.B4.Modules.States");
        }

        private void CheckReference(CheckBox checkBox, string reference)
        {
            if (checkBox.Checked && !_project.HasReference(reference))
            {
                MessageBox("В проекте нет ссылки на {0}!".F(reference), "Зависимости");
            }
        }

        private void tbEntityName_KeyUp(object sender, KeyEventArgs e)
        {
            if (_project != null)
            {
                if (e.KeyCode == Keys.Return)
                {
                    var match = _manager.ClassExists(tbEntityName.Text);
                    if (match.Any())
                    {
                        ClassBrowserWindow classWindow = new ClassBrowserWindow(match.Values);
                        if (classWindow.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            RestoreOptions(classWindow.Class);
                        }
                    }
                }
                else
                {
                    var ns = _project.DefaultNamespace();
                    var entityName = tbEntityName.Text;

                    tbpPrefix.Text = ns.Substring(5) + "." + entityName;
                    tbcName.Text = entityName;
                    tbTableName.Text = EntityHelper.TableNameByEntityName(entityName, ns);
                    tbvNamespace.Text = ns.Substring(5) + "." + entityName;

                    if (entityName.EndsWith("View"))
                    {
                        cbeBaseClass.Text = "PersitentObject";
                    }
                    else if (!entityName.EndsWith("View") && cbeBaseClass.Text == "PersitentObject")
                    {
                        cbeBaseClass.Text = "BaseEntity";
                    }

                    UpdateEditors();
                }
            }
        }

        private void cheOwnerReference_CheckedChanged(object sender, EventArgs e)
        {
            if (cheOwnerReference.Checked)
            {
                foreach (ListViewItem lvi in lvFields.Items)
                {
                    if (((FieldOptions)lvi.Tag).OwnerReference && ((FieldOptions)lvi.Tag).FieldName != tbeName.Text) 
                    {
                        MessageBox("Поле {0} уже назначено ссылкой на владельца!".F(((FieldOptions)lvi.Tag).FieldName), "Предупреждение");
                        cheOwnerReference.Checked = false;
                        break;
                    }
                }
                if (cheOwnerReference.Checked)
                {
                    cheNullable.Checked = false;
                }
                cheNullable.Enabled = !cheOwnerReference.Checked;
            }
        }

        private void cheParentReference_CheckedChanged(object sender, EventArgs e)
        {
            if (cheParentReference.Checked)
            {
                if (tbeType.Text != tbEntityName.Text)
                {
                    MessageBox("Тип поля должен совпадать с именем класса!", "Предупреждение");
                    cheParentReference.Checked = false;
                }
                else
                foreach (ListViewItem lvi in lvFields.Items)
                {
                    if (((FieldOptions)lvi.Tag).ParentReference && ((FieldOptions)lvi.Tag).FieldName != tbeName.Text)
                    {
                        MessageBox("Поле {0} уже назначено ссылкой на родителя!".F(((FieldOptions)lvi.Tag).FieldName), "Предупреждение");
                        cheParentReference.Checked = false;
                        break;
                    }

                    if (((FieldOptions)lvi.Tag).GroupField)
                    {
                        MessageBox("Иерархия невозможна в таблице с группировкой!", "Предупреждение");
                        cheParentReference.Checked = false;
                        break;
                    }
                }
                cheNullable.Checked = cheParentReference.Checked;
                cheNullable.Enabled = !cheParentReference.Checked;
            }
        }

        private void cheList_CheckedChanged(object sender, EventArgs e)
        {
            if (cheList.Checked)
            {
                cheOwnerReference.Enabled = cheParentReference.Enabled = cheNullable.Enabled = cheEnum.Enabled = 
                cheOwnerReference.Checked = cheParentReference.Checked = cheNullable.Checked = cheEnum.Checked = false;
            }
            else
            {
                cheOwnerReference.Enabled = cheParentReference.Enabled = cheNullable.Enabled = cheEnum.Enabled = true;
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
                        MessageBox("Поле {0} уже назначено группировкой таблицы!".F(((FieldOptions)lvi.Tag).FieldName), "Предупреждение");
                        chvGroupField.Checked = false;
                        break;
                    }

                    if (((FieldOptions)lvi.Tag).ParentReference)
                    {
                        MessageBox("Группировка невозможна в иерархической таблице!".F(((FieldOptions)lvi.Tag).FieldName), "Предупреждение");
                        chvGroupField.Checked = false;
                        break;
                    }
                }
            }
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ComposeOptions().Save(saveDialog.FileName);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                RestoreOptions(EntityOptionsExt.Load(openDialog.FileName));
            }
        }

        private void RestoreOptions(EntityOptions options)
        {
            _preventInterfaceActions = true;

            foreach (var field in options.Fields)
            {
                var lvif = lvFields.Items.Add(field.FieldName);
                lvif.Tag = field;
                lvif.SubItems.Add("");

                var lviv = lvView.Items.Add(field.FieldName);
                lviv.Tag = field;
                lviv.SubItems.Add("");
                lviv.SubItems.Add("");

                var lvim = lvMap.Items.Add(field.FieldName);
                lvim.Tag = field;
                lvim.SubItems.Add("");
            }

            options.Map(tbEntityName, x => x.ClassName);
            options.Map(chDictionary, x => x.IsDictionary);
            options.Map(tbTableName, x => x.TableName);
            options.Map(tbSubfolder, x => x.Subfolder);

            if (options.Controller != null)
                options.Map(tbcName, x => x.Controller.Name);

            if (options.View != null)
            {
                options.Map(tbvDisplayName, x => x.DisplayName);
                options.Map(tbvNamespace, x => x.View.Namespace);
                options.Map(chvEditingDisabled, x => x.View.EditingDisabled);
                options.Map(chvDynamicFilter, x => x.View.DynamicFilter);
                options.Map(cbvSelectionModel, x => x.View.SelectionModel);
                cbvViewType.SelectedIndex = (int)options.View.Type - 1;
                options.Map(chvTreeGrid, x => x.View.TreeGrid);
            }

            options.Map(tbMigrationVersion, x => x.MigrationVersion);

            if (options.Permission != null)
            {
                options.Map(tbpPrefix,        x => x.Permission.Prefix);
                options.Map(chpSimpleCRUDMap, x => x.Permission.SimpleCRUDMap);
            }

            if (options.Navigation != null)
            {
                options.Map(tbnRoot, x => x.Navigation.Root);
                options.Map(tbnName, x => x.Navigation.Name);
                options.Map(tbnAnchor, x => x.Navigation.Anchor);
                options.Map(chnMapPermissions, x => x.Navigation.MapPermission);
            }

            if (options.Interceptor != null)
            {
                options.Map(chdCreateBefore, x => x.Interceptor.Actions.Contains("BeforeCreate"));
                options.Map(chdCreateAfter, x => x.Interceptor.Actions.Contains("AfterCreate"));
                options.Map(chdUpdateBefore, x => x.Interceptor.Actions.Contains("BeforeUpdate"));
                options.Map(chdUpdateAfter, x => x.Interceptor.Actions.Contains("AfterUpdate"));
                options.Map(chdDeleteBefore, x => x.Interceptor.Actions.Contains("BeforeDelete"));
                options.Map(chdDeleteAfter, x => x.Interceptor.Actions.Contains("AfterDelete"));
            }

            if (options.DomainService != null)
            {
                options.Map(chdsSave, x => x.DomainService.Save);
                options.Map(chdsUpdate, x => x.DomainService.Update);
                options.Map(chdsDelete, x => x.DomainService.Delete);
                options.Map(chdsSaveInternal, x => x.DomainService.SaveInternal);
                options.Map(chdsUpdateInternal, x => x.DomainService.UpdateInternal);
                options.Map(chdsDeleteInternal, x => x.DomainService.DeleteInternal);
            }

            options.Map(chSignable, x => x.Signable);
            options.Map(chStateful, x => x.Stateful);
            options.Map(chmLogMap, x => x.AuditLogMap);

            _preventInterfaceActions = false;
            UpdateListViews();
            UpdateEditors();

            
        }

        private bool _preventInterfaceActions = false;

        private void MessageBox(string text, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Exclamation)
        {
            if (!_preventInterfaceActions)
                System.Windows.Forms.MessageBox.Show(text, caption, buttons, icon);
        }

        private void chDictionary_CheckedChanged(object sender, EventArgs e)
        {
            tbSubfolder.Enabled = !chDictionary.Checked;
        }
    }
}
