using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;
    using CodeGeneration.CSharp;
    using CodeGeneration.JavaScript;

    public class ViewGenerator : BaseBarsGenerator
    {
        public override List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var files = base.Generate(project, options, fragments);

            ControllerOptions controllerOpts = options.Controller;
            if (controllerOpts == null)
                controllerOpts = new ControllerOptions { Name = options.ClassName };

            switch (options.View.Type)
            {
                case ViewType.EAS: 
                    EASViewType(options, fragments, files.First(), controllerOpts, project);
                    return files;
                case ViewType.ViewModel: 
                    ViewModelViewType(options, fragments, files.First(), controllerOpts, project);
                    return files;
                case ViewType.B4:
                    B4ViewType(options, fragments, files, controllerOpts);
                    return files;
                default:
                    throw new NotSupportedException("Указанный тип представления не поддерживается");
            }
        }

        #region B4
        private void B4ViewType(EntityOptions options, GeneratedFragments fragments, List<GeneratedFile> files, ControllerOptions controllerOpts)
        {
            files.Clear();
            files.Add(B4Grid(options, fragments, controllerOpts));

            if (!options.View.EditingDisabled)
                files.Add(B4EditWindow(options, fragments, controllerOpts));
            
            files.Add(B4Page(options, fragments, controllerOpts));
        }

        private GeneratedFile B4Grid(EntityOptions options, GeneratedFragments fragments, ControllerOptions controllerOpts)
        {
            var file = new GeneratedFile { Generator = this, Properties = new Dictionary<string, object> { { "BuildAction", 3 } } };

            bool isTree = options.View.TreeGrid;

            var depList = new List<string>();
            depList.Add("EAS4.grid.Columns");
            depList.Add("EAS4.grid.Buttons");

            string extened = "EAS4.";
            string created = options.View.Namespace + ".";

            if (isTree)
            {
                extened = extened + "tree.Tree";
                created = created + "TreeGrid";
                
                depList.Add("EAS4.data.TreeStore");
                if (!options.View.EditingDisabled)
                    depList.Add("EAS4.tree.AddButton");
                depList.Add("EAS4.tree.UpdateButton");
            }
            else
            {
                extened = extened + "grid.Grid";
                created = created + "Grid";
                depList.Add("EAS4.data.Store");
            }
                        
            // store
            var storeFields = new JsArray();
            storeFields.Add("Id");
            foreach (var field in options.Fields.Where(x => !string.IsNullOrEmpty(x.DisplayName) && !x.Collection && !x.ParentReference))
                storeFields.Add(field.FieldName);

            var store = new JsInstance(
                isTree ? "EAS4.data.TreeStore" : "EAS4.data.Store", 
                new {
                    autoLoad = true,
                    fields = storeFields,
                    controllerName = JsScalar.New("config.controllerName")
                }
            );

            // columns
            var columns = new JsArray();

            bool first = true;

            if (!options.View.EditingDisabled)
                columns.Add(new JsInstance("EAS4.grid.Columns.GridEditColumn"));

            foreach (var field in options.Fields.Where(x => !string.IsNullOrEmpty(x.DisplayName) && !x.Collection && !x.ParentReference && !x.GroupField))
            {
                if (options.Stateful && field.FieldName == "State")
                {
                    continue;
                }
                else
                {
                    var col = (JsObject)new
                    {
                        __inline = true,
                        dataIndex = field.FieldName,
                        header = lc(field.DisplayName),
                        id = "column" + field.FieldName
                    }.ToJs();

                    if (field.TypeName == "bool")
                    {
                        col.AddScalar("renderer", "function (value) { return !!value ? 'да' : 'нет'; }");
                    }
                    else if (field.IsReference())
                    {
                        col.AddScalar("renderer", "function (value) {{ if (!value) return ''; return value.{0}; }}".R(field.TextProperty));
                    }
                    else
                    {
                        if (first && isTree)
                        {
                            col.Add("xtype", "treecolumn");
                            first = false;
                        }
                        else
                        col.Add("xtype", field.ViewColumnType);
                    }
                    columns.Values.Add(col);
                }
            }
            if (!options.View.EditingDisabled)
                columns.Add(new JsInstance("EAS4.grid.Columns.GridDeleteColumn"));
            
            var filter = new JsObject();
            foreach (var field in options.Fields.Where(x => !string.IsNullOrEmpty(x.DisplayName) && !x.Collection && !x.ParentReference && !x.GroupField))
            {
                var fil = new JsObject() { Name = field.FieldName, Inline = true };
                if (options.Stateful && field.FieldName == "State")
                {
                    continue;
                }
                else
                {
                    fil.Add("xtype", field.TypeName != "bool" ? (field.ViewType == "easselectfield" ? "textfield" : field.ViewType) : "combobox");
                    if (field.TypeName == "bool")
                    {
                        fil.AddScalar("items", "[[null, '-'], [true, 'да'], [false, 'нет']]");
                    }
                }
                filter.Add(fil);
            }

            var define = new JsFunctionCall();

            if (isTree)
            {
                define = Ext.define(created, new
                {
                    extend = extened,
                    requires = depList.ToArray(),
                    getTreeConfig = function("config",
                        "var thisGrid = this;",
                        "",
                        Return(Ext.apply(new { __inline = true }, new
                        {
                            selModel = new JsInstance("Ext.selection.{0}Model".R(options.View.SelectionModel.StartsWith("Checkbox") ? "Checkbox" : "Row")),
                            pagingEnabled = true,
                            rootVisible = false,
                            cls = "eas4-tree-wrap eas4-tree-noicon",
                            columns = columns,
                            store = new JsInstance("EAS4.data.TreeStore", new
                            {
                                autoLoad = true,
                                fields = storeFields,
                                controllerName = JsScalar.New("config.controllerName")
                            }),
                            root = new
                            {
                                expanede = true
                            },
                            tbarButtons = new[]
                            {
                                options.View.EditingDisabled ? null : New("EAS4.tree.AddButton"),
                                New("EAS4.tree.UpdateButton")
                            },
                            filter = filter
                        }, JsScalar.New("config")))
                    )
                });
            }
            else
            {
                define = Ext.define(created, new
                {
                    extend = extened,
                    requires = depList.ToArray(),
                    getGridConfig = function("config",
                        "var thisGrid = this;",
                        "",
                        Return(Ext.apply(new { __inline = true }, new
                        {
                            selModel = new JsInstance("Ext.selection.{0}Model".R(options.View.SelectionModel.StartsWith("Checkbox") ? "Checkbox" : "Row")),
                            columns = columns,
                            store = new JsInstance("EAS4.data.Store", new
                            {
                                autoLoad = true,
                                fields = storeFields,
                                controllerName = JsScalar.New("config.controllerName")
                            }),
                            tbarButtons = new[]{
                                New("EAS4.grid.Buttons.GridAddButton"),
                                New("EAS4.grid.Buttons.GridUpdateButton")
                            },
                            filter = filter
                        }, JsScalar.New("config")))
                    )
                });
            }

            file.Body = define.Draw(0);
            file.Name = (options.View.TreeGrid ? "Tree" : "") +  "Grid.js";
            file.Path = "libs\\" + (options.IsDictionary ? "Dict\\" : (!string.IsNullOrWhiteSpace(options.Subfolder) ? options.Subfolder : ""));
            return file;
        }

        private GeneratedFile B4EditWindow(EntityOptions options, GeneratedFragments fragments, ControllerOptions controllerOpts)
        {
            var file = new GeneratedFile { Generator = this, Properties = new Dictionary<string, object> { { "BuildAction", 3 } } };

            var depList = new JsArray();

            if (options.AcceptFiles)
                depList.Add("EAS4.form.FileUpload");

            if (options.Fields.Any(x => x.IsReference()))
                depList.Add("EAS4.form.SelectField");

            var formItems = new JsArray();

            foreach (var field in options.Fields.Where(x => !string.IsNullOrEmpty(x.DisplayName) && !x.Collection))
            {
                if (options.Stateful && field.FieldName == "State")
                {
                    continue;
                }
                else
                    if (field.ViewType == "easselectfield")
                    {
                        formItems.Values.Add(new
                        {
                            xtype = field.ViewType,
                            fieldLabel = lc(field.DisplayName),
                            dataIndex = field.FieldName,
                            idProperty = "Id",
                            textProperty = field.TextProperty,
                            selectWindowConfig = new
                            {
                                title = field.DisplayName,
                                storeConfig = new
                                {
                                    fields = new object[] { JsStyle.Inline, "Id", field.TextProperty },
                                    controllerName = field.TypeName
                                },
                                gridConfig = new
                                {
                                    columns = new[]{ new{ 
                                    __inline = true,
                                    dataIndex = field.TextProperty, 
                                    header = lc(field.DisplayName), 
                                    xtype = "easwraptextcolumn" 
                                }}
                                }
                            },
                            __inline = false
                        }.ToJs());
                    }
                    else
                    {
                        formItems.Values.Add(new
                        {
                            xtype = field.ViewType,
                            fieldLabel = lc(field.DisplayName),
                            dataIndex = field.FieldName,
                            allowBlank = field.Nullable
                        }.ToJs());
                    }
            }

            var define = Ext.define(options.View.Namespace + ".EditWindow", new {
                extend = "EAS4.form.EditWindow",
                requires = depList,
                title = lc(options.DisplayName),
                width = 800,
                saveAndClose = true,
                initComponent = function(string.Empty,
                    Ext.apply(JsScalar.This, new {
                        items = new object[] { new {
                            xtype = "form",
                            border = false,
                            fileUpload = options.AcceptFiles,
                            defaults = new { labelWidth = 130, anchor = "100%" },
                            items = formItems
                        }}
                    }),
                    "",
                    new JsFunctionCall("this.callParent", new[] { JsScalar.New("config") })
                )
            });

            file.Body = define.Draw(0);
            file.Name = "EditWindow.js";
            file.Path = "libs\\" + (options.IsDictionary ? "Dict\\" : (!string.IsNullOrWhiteSpace(options.Subfolder) ? options.Subfolder : ""));
            return file;
        }

        private GeneratedFile B4Page(EntityOptions options, GeneratedFragments fragments, ControllerOptions controllerOpts)
        {
            var file = new GeneratedFile { Generator = this, Properties = new Dictionary<string, object> { { "BuildAction", 3 } } };

            var depList = new JsArray();

            if (options.Permission != null)
            {
                depList.Add("EAS4.permissions.GenericDictionaryPermissionsPlugin");
                depList.Add("EAS4.permissions.Apply");
            }

            if (!options.View.EditingDisabled)
            {
                depList.Add("EAS4.page.plugins.GridEditWindowPlugin");
                depList.Add(options.View.Namespace + ".EditWindow");
            }
            depList.Add(options.View.Namespace + ".Grid");

            var initPageContent = new List<object>
            {
                new JsFunctionCall("this.callParent"),
                "",
                this.addMainComponent("grid", new JsInstance(options.View.Namespace + ".Grid"))
            };

            if (!options.View.EditingDisabled)
            {
                initPageContent.Add(this.addComponent("editWindow", new JsInstance(options.View.Namespace + ".EditWindow")));
                initPageContent.Add("");
                initPageContent.Add(this.addPlugin( 
                    new JsInstance("EAS4.page.plugins.GridEditWindowPlugin", new 
                    {
                        controllerName = JsScalar.New("this.controllerName"),
                        gridName = "grid",
                        windowName = "editWindow"
                    }).NotInline
                ));
            }

            if (options.Permission != null)
            {
                initPageContent.Add("");
                initPageContent.Add(this.addPlugin(
                    new JsInstance("EAS4.permissions.GenericDictionaryPermissionsPlugin", new 
                    {
                        permissionsNamespace = options.Permission.Prefix,
                        gridName = "grid",
                        windowName = !options.View.EditingDisabled ? "editWindow" : null,
                        addButtonApplyBy = JsScalar.New("EAS4.permissions.Apply.DisableAndHide")
                    }).NotInline
                ));
            }

            var define = Ext.define(options.View.Namespace + ".Page", new
            {
                extend = "EAS4.page.Page",
                requires = depList,
                title = lc(options.DisplayName),
                controllerName = options.Controller.Name,
                permissionNamespace = options.Permission != null ? options.Permission.Prefix : null,
                initPage = function(string.Empty, initPageContent.ToArray())
            });

            file.Body = define.Draw(0);
            file.Name = "Page.js";
            file.Path = "libs\\" + (options.IsDictionary ? "Dict\\" : (!string.IsNullOrWhiteSpace(options.Subfolder) ? options.Subfolder : ""));
            return file;
        }
        #endregion

        #region EAS
        private void EASViewType(EntityOptions options, GeneratedFragments fragments, GeneratedFile file, ControllerOptions controllerOpts, ProjectInfo project)
        {
            var aFunction = new JsFunction() { Inline = false };

            var ns = new JsFunctionCall("Ext3.ns", new[] { options.View.Namespace ?? "" }) { Name = "var ns", Inline = true };
            
            aFunction.Add(ns);
            aFunction.Add("");
            aFunction.Add(EASGrid(options, project));

            if (!options.View.EditingDisabled)
            {
                aFunction.Add("");
                aFunction.Add(EASEditWindow(options, controllerOpts.Name, project));
            }
            aFunction.Add("");
            aFunction.Add(EASPage(options, controllerOpts.Name));
            aFunction.Add("");
            aFunction.Add("return ns.Page;");

            var deps = new List<object>();

            if (options.Signable)
                deps.Add("modules/MosKs/DigitalSignature/MosKs.SignatureEntity");

            if (options.View.DynamicFilter)
                deps.Add("modules/MosKs.Plugin.QueryBuilder");

            if (options.View.TreeGrid)
                deps.Add("modules/MosKs.Controls.TreeGrid");

            if (options.Stateful)
            {
                deps.Add("modules/EAS.States.StateField");
                deps.Add("modules/EAS.States.StateColumn");
                deps.Add("modules/EAS.States.PageGridStatePlugin");
                deps.Add("modules/EAS.States.StateFilterCombobox");
            }

            var define = new JsFunctionCall { Function = "define" };
            define.Params.Add(deps.ToArray().ToJs());
            define.Params.Add(aFunction);

            file.Name = options.ClassName + ".js";
            file.Path = "Views\\" + (options.IsDictionary ? "Dict\\" : (!string.IsNullOrWhiteSpace(options.Subfolder) ? options.Subfolder : ""));
            file.Body = define.Draw(0);
            file.Properties.Add("BuildAction", 3);

            var resources = new GeneratedFragment
            {
                FileName = "ResourceManifest.cs",
                InsertToFile = true,
                InsertClass = "public class ResourceManifest",
                InsertMethod = "public void InitManifests(IResourceManifestContainer container)",
                Generator = this
            };
            resources.Lines.Add("container.Add(\"scripts/modules/{3}.js\", \"{0}.dll/{0}.Views.{2}{1}.js\");".R(project.DefaultNamespace, options.ClassName, options.IsDictionary ? "Dict." : "", options.View.Namespace));

            fragments.Add("ResourceManifest.cs", resources);
        }
        
        private JsFunctionCall EASGrid(EntityOptions options, ProjectInfo project)
        {
            var gridConfig = new JsFunctionCall { Function = "Ext3.apply", Name = "return" };

            gridConfig.AddParam(new { __inline = true });
            gridConfig.AddParam(JsScalar.New("config"));
            
            var gridApply = new JsObject();

            JsProperty sm = null;
            if (options.View.SelectionModel.StartsWith("Checkbox"))
            {
                sm = JsScalar.New("sm");
            }
            else
            {
                sm = new JsInstance("Ext3.grid." + options.View.SelectionModel);
            }
            gridApply.Add("sm", sm);

            var store = new JsInstance(options.View.TreeGrid ? "Ext3.ux.maximgb.tree.AdjacencyListStore" : "EAS.Store");
            
            var storeFields = new JsArray() { Inline = true };
            storeFields.Add("Id");
            foreach (var field in options.Fields.Where(x => !string.IsNullOrEmpty(x.DisplayName) && !x.Collection && !x.ParentReference))
                storeFields.Add(field.FieldName);

            if (options.Signable)
                storeFields.Add("Signed");

            if (options.View.TreeGrid)
            {
                gridApply.Add("master_column_id", "column" + options.Fields.First(x => !string.IsNullOrEmpty(x.DisplayName) && !x.Collection && !x.GroupField).FieldName);


                storeFields.Add("_parent");
                storeFields.Add("_is_leaf");
                storeFields.Add("_is_loaded");

                var storeParamss = new
                {
                    autoLoad = true,
                    remoteSort = true,
                    reader = new JsInstance("Ext3.data.JsonReader",
                        new 
                        { 
                            idProperty = "Id", 
                            root = "data", 
                            totalProperty = "totalCount",
                            fields = storeFields
                        }
                    ),
                    proxy = new JsInstance("Ext3.data.HttpProxy",
                        new 
                        { 
                            __inline = true,
                            method = "POST", 
                            url = JsScalar.New("EAS.url.action('/' + config.controllerName + '/List/')"), 
                            json = true
                        }
                    ),
                    baseParams = new { start = 0, limit = 20 }
                };
                store.Params.Add(storeParamss.ToJs());
            }
            else
            {
                var storeParams = (JsObject)new
                    {
                        fields = storeFields,
                        controllerName = JsScalar.New("config.controllerName")
                    }.ToJs();

                var groupField = options.Fields.FirstOrDefault(x => x.GroupField);
                if (groupField != null)
                {
                    storeParams.Add("groupField", groupField.FieldName);

                    gridApply.Add("view", new JsInstance
                    {
                        Function = "Ext3.grid.GroupingView",
                        Inline = true
                    }.AddParam(new { __inline = true, hideGroupedColumn = true }));

                    
                }
                store.Params.Add(storeParams);
            }

            gridApply.Add("store", store);

            var columns = new JsArray();

            if (options.View.SelectionModel.StartsWith("Checkbox"))
                columns.AddScalar("sm");

            columns.Values.Add(new JsInstance("EAS.GridEditColumn"));

            if (options.Signable)
            {
                var signColumn = new {
                    __inline = false,
                    xtype = "easgridactionscolumn",
                    id = "columnSign",
                    getRowActions = new JsFunction { 
                        Params = "value, meta, record", 
                        Body = new List<object> { "return [{ name: 'SignData', iconCls: (record.data.Signed ? 'icon-signed' : '') }];" } },
                    header = lc("ЭЦП")
                };

                columns.Values.Add(signColumn.ToJs());
            }

            foreach (var field in options.Fields.Where(x => !string.IsNullOrEmpty(x.DisplayName) && !x.Collection && !x.ParentReference && !x.GroupField))
            {
                if (options.Stateful && field.FieldName == "State")
                {
                    var col = new JsInstance(
                        "EAS.States.StateColumn",
                        new
                        {
                            __inline = true,
                            dataIndex = "State",
                            header = lc("Статус"),
                            width = 100,
                            @fixed = true
                        }
                    ) { Inline = true };
                    columns.Values.Add(col);
                }
                else if (field.TypeName == "FileInfo")
                {
                    var col = (JsObject)new
                    {
                        __inline = true,
                        dataIndex = field.FieldName,
                        header = lc(field.DisplayName),
                        id = "column" + field.FieldName
                    }.ToJs();

                    col.AddScalar("renderer", "function (v) { if (v) return '<a href=\"' + EAS.url.action('/FileUpload/Download?id=' + v) + '\" target=\"_blank\">\'+lc('Скачать')+'</a>'; return ''; }");
                    columns.Values.Add(col);
                }
                else
                {
                    var col = (JsObject)new
                    {
                        __inline = true,
                        dataIndex = field.FieldName,
                        header = lc(field.DisplayName),
                        id = "column" + field.FieldName
                    }.ToJs();
                    
                    if (field.TypeName == "bool")
                    {
                        col.AddScalar("renderer", "function (value) { return !!value ? 'да' : 'нет'; }");
                    }
                    else if (field.Enum)
                    {
                        col.AddScalar("renderer", "{1}.Enum.{0}.getRenderer()".R(field.TypeName, project.DefaultNamespace.CutFirst(5)));
                    }
                    else if (field.IsReference())
                    {
                        col.AddScalar("renderer", "function (value) {{ if (!value) return ''; return value.{0}; }}".R(field.TextProperty));
                    }
                    else
                    {
                        col.Add("xtype", field.ViewColumnType);
                    }
                    columns.Values.Add(col);
                }
            }

            columns.Values.Add(new JsInstance("EAS.GridDeleteColumn"));

            gridApply.Add("columns", columns);

            var tbarButtons = new JsArray() { Name = "tbarButtons" };

            if (!options.View.EditingDisabled)
            tbarButtons.Values.Add(new JsInstance("EAS.GridAddButton"));

            tbarButtons.Values.Add(new JsInstance("EAS.GridUpdateButton"));
            gridApply.Add(tbarButtons);

            var filter = new JsObject() { Name = "filter" };
            foreach (var field in options.Fields.Where(x => !string.IsNullOrEmpty(x.DisplayName) && !x.Collection && !x.ParentReference && !x.GroupField))
            {
                var fil = new JsObject() { Name = field.FieldName, Inline = true };
                if (options.Stateful && field.FieldName == "State")
                {
                    fil.Add("xtype", "statefiltercombobox");
                    fil.Add("stateTypeId", options.ClassFullName);
                    fil.Add("editable", false);
                } else 
                if (field.Enum)
                {
                    fil.Add("xtype", "eascombobox");
                    fil.Add("editable", false);
                    fil.AddScalar("value", "null");
                    fil.AddScalar("items", "{1}.Enum.{0}.getItems()".R(field.TypeName, project.DefaultNamespace.CutFirst(5)));
                    fil.Add("nullItemAdd", true);
                }
                else if (field.TypeName == "FileInfo")
                {
                    continue;
                }
                else
                {
                    fil.Add("xtype", field.TypeName != "bool" ? (field.ViewType == "easselectfield" ? "textfield" : field.ViewType) : "combobox");
                    if (field.TypeName == "bool")
                    {
                        fil.AddScalar("items", "[[null, '-'], [true, 'да'], [false, 'нет']]");
                    }
                }
                filter.Add(fil);
            }
            gridApply.Add(filter);

            gridConfig.Params.Add(gridApply);

            var getGridConfig = new JsFunction() { Name = "getGridConfig" };
            getGridConfig.Params = "config";

            if (options.View.SelectionModel.StartsWith("Checkbox"))
                getGridConfig.Add(new JsInstance { Name = "sm", Function = "Ext3.grid." + options.View.SelectionModel });

            getGridConfig.Add("var thisGrid = this;");
            getGridConfig.Add(gridConfig);

            var extendParams = new JsObject();
            extendParams.Properties.Add(getGridConfig);

            var gridExtend = new JsFunctionCall() { Name = "ns.Grid", Function = "Ext3.extend" };
            gridExtend.Params.Add(new JsScalar() { Value = !options.View.TreeGrid ? "EAS.Grid" : "MosKs.Controls.TreeGrid" });
            gridExtend.Params.Add(extendParams);
            return gridExtend;
        }

        private JsFunctionCall EASEditWindow(EntityOptions options, string controllerName, ProjectInfo project)
        {
            var extendParams = new JsObject();

            extendParams.Add("width", 600);
            extendParams.Add("title", lc(options.View.Title));
            extendParams.Add("padding", 5);

            var init = new JsFunction() { Name = "initComponent" };
            init.Add("ns.EditWindow.superclass.initComponent.call(this);");
            init.Add("");
            init.Add("var thisWindow = this;");

            if (options.Signable)
            {
                init.Add("this.addEvents('signButtonClick');");

                var signButton = new JsFunctionCall("this.saveButtonGroup.add",
                    new
                    {
                        xtype = "button",
                        text = lc("ЭЦП"),
                        iconCls = "icon-signed",
                        @ref = "signButton",
                        listeners = new
                        {
                            fn = new JsFunction("btn", "thisWindow.fireEvent('signButtonClick');"),
                            scope = JsScalar.New("this")
                        }
                    }
                );
                init.Add(signButton);
            }

            var fields = new JsArray(){ Name = "items" };
            foreach (var field in options.Fields.Where(x => !string.IsNullOrEmpty(x.DisplayName) && !x.Collection))
            {
                if (options.Stateful && field.FieldName == "State")
                {
                    fields.Values.Add(new {
                        xtype = field.ViewType,
                        fieldLabel = lc(field.DisplayName),
                        @ref = "../field_State",
                        controllerName = controllerName,
                        entityTypeId = JsScalar.New("this.entityTypeId")
                    }.ToJs());
                } else
                if (field.Enum)
                {
                    fields.Values.Add(new
                    {
                        xtype = field.ViewType,
                        dataIndex = field.FieldName,
                        fieldLabel = lc(field.DisplayName),
                        editable = false,
                        allowBlank = field.Nullable,
                        items = JsScalar.New("{1}.Enum.{0}.getItems()".R(field.TypeName, project.DefaultNamespace.CutFirst(5))),
                        defaultValue = JsScalar.New("{1}.Enum.{0}.getDefaultValue()".R(field.TypeName, project.DefaultNamespace.CutFirst(5)))
                    }.ToJs());
                } else
                if (field.ViewType == "easselectfield")
                {
                    fields.Values.Add(new {
                        xtype = field.ViewType,
                        fieldLabel = lc(field.DisplayName),
                        dataIndex = field.FieldName,
                        idProperty = "Id",
                        textProperty = field.TextProperty,
                        selectWindowConfig = new {
                            title = field.DisplayName,
                            storeConfig = new { 
                                fields = new object[]{ JsStyle.Inline, "Id", field.TextProperty },
                                controllerName = field.TypeName 
                            },
                            gridConfig = new { 
                                columns = new []{ new{ 
                                    __inline = true,
                                    dataIndex = field.TextProperty, 
                                    header = lc(field.DisplayName), 
                                    xtype = "easwraptextcolumn" 
                                }}
                            }
                        },
                        __inline = false
                    }.ToJs());
                }
                else
                {
                    fields.Values.Add(new
                    {
                        xtype = field.ViewType,
                        fieldLabel = lc(field.DisplayName),
                        dataIndex = field.FieldName,
                        allowBlank = field.Nullable
                    }.ToJs());
                }
            }
            
            var add = new JsFunctionCall() { Function = "this.add" };
            add.Params.Add(new {
                xtype = "form",
                padding = 5,
                fileUpload = options.AcceptFiles,
                items = fields
            }.ToJs());

            init.Add(add);

            if (options.Stateful)
            {
                var resetHandler = new JsFunctionCall { Function = "this.on", Params = new List<JsProperty> { new JsScalar { Value = "'resetfields'" } }, Inline = false };
                resetHandler.Params.Add(new JsFunction { Params = "win", Body = new List<object> { "win.field_State.setStatus(null);" } });
                init.Add(resetHandler);

                var createHandler = new JsFunctionCall { Function = "this.on", Params = new List<JsProperty> { new JsScalar { Value = "'createsuccess'" } }, Inline = false };
                createHandler.Params.Add(new JsFunction { Params = "win, data", Body = new List<object> { "win.field_State.setStatus(data.Id, data.State);" } });
                init.Add(createHandler);

                var setdataHandler = new JsFunctionCall { Function = "this.on", Params = new List<JsProperty> { new JsScalar { Value = "'setdata'" } }, Inline = false };
                setdataHandler.Params.Add(new JsFunction { Params = "win, id, data", Body = new List<object> { "if (id) { win.field_State.setStatus(id, data.State); }" } });
                init.Add(setdataHandler);
            }

            extendParams.Properties.Add(init);

            var editWindowExtend = new JsFunctionCall() { Name = "ns.EditWindow", Function = "Ext3.extend" };
            editWindowExtend.Params.Add(new JsScalar() { Value = "EAS.EditWindow" });
            editWindowExtend.Params.Add(extendParams);
            return editWindowExtend;
        }

        private JsFunctionCall EASPage(EntityOptions options, string controllerName)
        {
            var extendParams = new JsObject();

            extendParams.Add("title", lc(options.View.Title) );
            extendParams.Add("controllerName", controllerName);

            if (options.Permission != null)
                extendParams.Add("permissionsNamespace", options.Permission.Prefix);

            if (options.Stateful || options.Signable)
                extendParams.Add("entityTypeId", options.ClassFullName);
            
            var initPage = new JsFunction();
            initPage.Add("ns.Page.superclass.initPage.call(this);");
            initPage.Add("");
            initPage.Add("var thisPage = this;");

            initPage.Add(this.addMainComponent("grid", 
                New("ns.Grid", new { controllerName = JsScalar.New("this.controllerName") }).NotInline
            ));
            
            if (options.Signable)
            {
                initPage.Add(this.addComponent(
                    "signatureWindow",
                    New("MosKs.SignatureEntity.Page", new { 
                        __inline = true, 
                        controllerName = "DocumentDigSignature", 
                        controllerAction = "SignList" 
                    })
                ));
            }

            if (!options.View.EditingDisabled)
            {
                var editParams = new JsObject();

                if (options.Stateful)
                { 
                    editParams.AddScalar("entityTypeId", "this.entityTypeId");
                }

                initPage.Add(this.addComponent("editWindow",
                    new JsInstance("ns.EditWindow",new []{ editParams }){ Inline = !editParams.Properties.Any() }
                ));
                if (options.Signable)
                {
                    initPage.Add("this.components.editWindow.on('signButtonClick', this.onSignButtonClick, this);");
                }
            }
            initPage.Add("");
            
            if (options.Stateful)
            {
                initPage.Add(this.addPlugin(
                    New("EAS.States.PageGridStatePlugin",
                        new {
                            gridName = "grid",
                            controllerName = "this.controllerName",
                            entityTypeId = "this.entityTypeId"
                        }
                    ).NotInline
                ));
            }

            if (options.View.DynamicFilter)
            { 
                initPage.Add(this.addPlugin(
                    New("MosKs.Plugin.QueryBuilder",
                        new {
                            gridName = "grid",
                            queryBuilderName = "queryBuilder",
                            entityName = options.ClassName
                        }
                    ).NotInline
                ));
            }

            if (!options.View.EditingDisabled)
            {
                initPage.Add(this.addPlugin(
                    New("EAS.PageGridEditWindowPlugin", 
                        new {
                            controllerName = "this.controllerName",
                            gridName = "grid",
                            windowName = "editWindow"
                        }
                    ).NotInline
                ));
            }

            if (options.Permission != null)
            {
                var pluginParams = (JsObject)new
                {
                    permissionsNamespace = "this.permissionsNamespace",
                    gridName = "grid"
                }.ToJs();

                if (!options.View.EditingDisabled)
                    pluginParams.Properties.Add(new JsScalar { Name = "windowName", Value = "'editWindow'" });

                initPage.Add(this.addPlugin(
                    New("EAS.Permissions.GenericDictionaryPermissionsPlugin", 
                        pluginParams).NotInline)
                );
            }
            extendParams.Add("initPage", initPage);

            if (options.Signable)
            {
                var signHandler = function("",
                    "this.components.signatureWindow.grid = this.components.grid;",
                    "this.components.signatureWindow.editWnd = this.components.editWindow;",
                    "this.components.signatureWindow.show(this.components.editWindow.objectId.value, this.entityTypeId, 'Подпись {0}');".R(options.DisplayName)
                );
                extendParams.Add("onSignButtonClick", signHandler);
            }

            var pageExtend = new JsFunctionCall() { Name = "ns.Page", Function = "Ext3.extend" };
            pageExtend.Params.Add(new JsScalar() { Value = "EAS.Page" });
            pageExtend.Params.Add(extendParams);
            return pageExtend;
        }
        #endregion

        private void ViewModelViewType(EntityOptions options, GeneratedFragments fragments, GeneratedFile file, ControllerOptions controllerOpts, ProjectInfo project)
        {
            var ns = new NamespaceInfo();
            var cls = new ClassInfo();

            ns.Name = "{0}.ViewModels".R(project.DefaultNamespace);

            ns.InnerUsing.Add("B4");
            ns.InnerUsing.Add("B4.Modules.Templates");
            ns.InnerUsing.Add("{0}.Entities".R(project.DefaultNamespace));

            ns.NestedValues.Add(cls);

            cls.Name = "{0}ViewModel".R(options.ClassName);
            cls.BaseClass = "ViewModel<{0}>".R(options.ClassName);

            _knownTypes.Add("ViewModel");
            _knownTypes.Add(options.ClassName);
            _knownTypes.Add("{0}ViewModel".R(options.ClassName));

            var ctor = new MethodInfo()
            {
                IsConstructor = true,
                Name = cls.Name
            };

            ctor.Body.Add("View(\"{0}\".Localize());".R(options.DisplayName));

            foreach (var field in options.Fields)
            {
                ctor.Body.Add("Property(x => x.{0}, \"{1}\".Localize());".R(field.FieldName, field.DisplayName));
            }
            ctor.Body.Add("Controller(\"{0}\");".R(controllerOpts.Name));
            ctor.Body.Add("InlineEdit();");

            cls.AddMethod(ctor);

            var resources = new GeneratedFragment
            {
                FileName = "ResourceManifest.cs",
                InsertToFile = true,
                InsertClass = "public class ResourceManifest",
                InsertMethod = "public void InitManifests(IResourceManifestContainer container)",
                Generator = this
            };
            resources.Lines.Add("container.Add(\"scripts/modules/{0}.{1}.js\", new GridPageView<{1}ViewModel>());".R(project.DefaultNamespace, options.ClassName, options.IsDictionary ? "Dict." : ""));

            fragments.Add("ResourceManifest.cs", resources);

            file.Name = options.ClassName + "ViewModel.cs";
            file.Path = "ViewModels\\" + (options.IsDictionary ? "Dict\\" : "");
            file.Body = ns.Generate();
        }

        #region js/extJs/EAS helpers
        private JsScalar lc(string localString, string name = "")
        { 
            return new JsScalar{ Name = name, Value = "lc('{0}')".R(localString) };
        }

        private JsFunction function(string @params, params object[] body)
        {
            return new JsFunction(@params, body);
        }

        private JsProperty Return(JsProperty value)
        {
            value.Name = "return";
            return value;
        }

        private JsInstance New(string function, params object[] @params)
        {
            return new JsInstance(function, @params);
        }

        private static class Ext
        {
            public static JsFunctionCall create(params object[] @params)
            {
                return new JsFunctionCall("Ext.create", @params);
            }

            public static JsFunctionCall define(params object[] @params)
            {
                return new JsFunctionCall("Ext.define", @params);
            }

            public static JsFunctionCall apply(params object[] @params)
            {
                return new JsFunctionCall("Ext.apply", @params);
            }
        }

        private JsFunctionCall addMainComponent(params object[] @params)
        {
            return new JsFunctionCall("this.addMainComponent", @params);
        }

        private JsFunctionCall addComponent(params object[] @params)
        {
            return new JsFunctionCall("this.addComponent", @params);
        }

        private JsFunctionCall addPlugin(params object[] @params)
        {
            return new JsFunctionCall("this.addPlugin", @params);
        }
        #endregion
    }
}
