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
        public override GeneratedFile Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var file = base.Generate(project, options, fragments);

            ControllerOptions controllerOpts = options.Controller;
            if (controllerOpts == null)
                controllerOpts = new ControllerOptions { Name = options.ClassName };

            if (!options.View.Inline)
            {
                var aFunction = new JsFunction() { Inline = false };

                var ns = new JsFunctionCall() { Function = "Ext3.ns", Name = "var ns", Inline = true };
                ns.Params.Add(JsScalar.String(options.View.Namespace));

                aFunction.Add(ns);
                aFunction.Add("");
                aFunction.Add(nsGrid(options));

                if (!options.View.EditingDisabled)
                {
                    aFunction.Add("");
                    aFunction.Add(nsEditWindow(options, controllerOpts.Name));
                }
                aFunction.Add("");
                aFunction.Add(nsPage(options, controllerOpts.Name));
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
                
                fragments.AddLines("ResourceManifest.cs", this, new List<string> { 
                    "container.Add(\"scripts/modules/{3}.js\", \"{0}.dll/{0}.Views.{2}{1}.js\");".F(_project.DefaultNamespace, options.ClassName, options.IsDictionary ? "Dict." : "", options.View.Namespace)} );

                return file;
            }
            else
            {
                var ns = new NamespaceInfo();
                var cls = new ClassInfo();

                ns.Name = "{0}.ViewModels".F(_project.DefaultNamespace);

                ns.InnerUsing.Add("B4");
                ns.InnerUsing.Add("B4.Modules.Templates");
                ns.InnerUsing.Add("{0}.Entities".F(_project.DefaultNamespace));

                ns.NestedValues.Add(cls);

                cls.Name = "{0}ViewModel".F(options.ClassName);
                cls.BaseClass = "ViewModel<{0}>".F(options.ClassName);

                var ctor = new MethodInfo()
                {
                    IsConstructor = true,
                    Name = cls.Name
                };

                ctor.Body.Add("View(\"{0}\".Localize());".F(options.DisplayName));
                                
                foreach (var field in options.Fields)
                {
                    ctor.Body.Add("Property(x => x.{0}, \"{1}\".Localize());".F(field.FieldName, field.DisplayName));
                }
                ctor.Body.Add("Controller(\"{0}\");".F(controllerOpts.Name));
                ctor.Body.Add("InlineEdit();");

                cls.AddMethod(ctor);
                
                fragments.AddLines("ResourceManifest.cs", this, new List<string> { 
                    "container.Add(\"scripts/modules/{0}.{1}.js\", new GridPageView<{1}ViewModel>());".F(_project.DefaultNamespace, options.ClassName, options.IsDictionary ? "Dict." : "")});

                file.Name = options.ClassName + "ViewModel.cs";
                file.Path = "ViewModels\\" + (options.IsDictionary ? "Dict\\" : "");
                file.Body = ns.Generate();
                return file;
            }
        }

        private JsFunctionCall nsGrid(EntityOptions options)
        {
            var gridConfig = new JsFunctionCall { Function = "Ext3.apply", Name = "return" };

            gridConfig.AddParam(new { __inline = true });
            gridConfig.AddParam(JsScalar.New("config"));
            
            var gridApply = new JsObject();

            JsProperty sm = null;
            if (options.View.SelectionModel.StartsWith("Checkbox"))
            {
                sm = new JsScalar { Name = "sm", Value = "sm" };
            }
            else
            {
                sm = new JsInstance { Name = "sm", Function = "Ext3.grid." + options.View.SelectionModel };
            }
            gridApply.Add(sm);



            var store = new JsInstance { Name = "store", Function = options.View.TreeGrid ? "Ext3.ux.maximgb.tree.AdjacencyListStore" : "EAS.Store" };
            //var storeParams = new JsObject();
            
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
                    reader = new JsInstance
                    {
                        Function = "Ext3.data.JsonReader",
                        Params = new List<JsProperty> {
                            new { 
                                idProperty = "Id", 
                                root = "data", 
                                totalProperty = "totalCount",
                                fields = storeFields
                            }.ToJs() 
                        }
                    },
                    proxy = new JsInstance
                    {
                        Function = "Ext3.data.HttpProxy",
                        Params = new List<JsProperty>{
                            new { 
                                __inline = true,
                                method = "POST", 
                                url = JsScalar.New("EAS.url.action('/' + config.controllerName + '/List/')"), 
                                json = true
                            }.ToJs()
                        }
                    },
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

            gridApply.Add(store);

            var columns = new JsArray() { Name = "columns" };

            if (options.View.SelectionModel.StartsWith("Checkbox"))
                columns.AddScalar("sm");

            columns.Values.Add(new JsInstance { Function = "EAS.GridEditColumn" });

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
                    var col = new JsInstance
                    {
                        Function = "EAS.States.StateColumn",
                        Inline = true
                    }.AddParam(new { 
                        __inline = true, 
                        dataIndex = "State", 
                        header = lc("Статус"), 
                        width = 100, 
                        @fixed = true 
                    });

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
                    else if (field.IsReference())
                    {
                        col.AddScalar("renderer", "function (value) {{ if (!value) return ''; return value.{0}; }}".F(field.TextProperty));
                    }
                    else
                    {
                        col.Add("xtype", field.ViewColumnType);
                    }
                    columns.Values.Add(col);
                }
            }

            columns.Values.Add(new JsInstance { Function = "EAS.GridDeleteColumn" });

            gridApply.Add(columns);

            var tbarButtons = new JsArray() { Name = "tbarButtons" };

            if (!options.View.EditingDisabled)
            tbarButtons.Values.Add(new JsInstance { Function = "EAS.GridAddButton" });

            tbarButtons.Values.Add(new JsInstance { Function = "EAS.GridUpdateButton" });
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

        private JsFunctionCall nsEditWindow(EntityOptions options, string controllerName)
        {
            var extendParams = new JsObject();

            extendParams.Add("width", 600);
            extendParams.Add("title", lc(options.View.Title));
            extendParams.Add("padding", 5);

            var init = new JsFunction() { Name = "initComponent" };
            init.Add("ns.EditWindow.superclass.initComponent.call(this);");
            init.Add("");
            init.Add("var thisWindow = this;");
            init.Add("this.addEvents('signButtonClick');");

            if (options.Signable)
            {
                var signButton = new JsFunctionCall { Function = "this.saveButtonGroup.add" };
                var signParams = new JsObject();
                signParams.Add("xtype", "button");
                signParams.Add("text", lc("ЭЦП"));
                signParams.Add("iconCls", "icon-signed");
                signParams.Add("ref", "signButton");
                signParams.Properties.Add(new JsObject
                {
                    Name = "listeners",
                    Properties = new List<JsProperty> { 
                        new JsObject{ Name = "click", Properties = new List<JsProperty>{
                            new JsFunction{ Name = "fn", Params = "btn", Body = new List<object>{ "thisWindow.fireEvent('signButtonClick');"}},
                            new JsScalar{ Name = "scope", Value = "this"}
                        }}
                    }
                });
                signButton.Params.Add(signParams);
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

        private JsFunctionCall nsPage(EntityOptions options, string controllerName)
        {
            var extendParams = new JsObject();

            extendParams.Add("title", lc(options.View.Title) );
            extendParams.Add("controllerName", controllerName);

            if (options.Permission != null)
                extendParams.Add("permissionsNamespace", options.Permission.Prefix);

            if (options.Stateful || options.Signable)
                extendParams.Add("entityTypeId", options.ClassFullName);
            
            var init = new JsFunction() { Name = "initPage" };
            init.Add("ns.Page.superclass.initPage.call(this);");
            init.Add("");
            init.Add("var thisPage = this;");

            var addMain = new JsFunctionCall { Function = "this.addMainComponent" };
            addMain.Params.Add(new JsScalar() { Value = "'grid'" });
            addMain.Params.Add(new JsInstance()
            {
                Inline = false,
                Function = "ns.Grid",
                Params = new List<JsProperty>{ new { controllerName = JsScalar.New("this.controllerName") }.ToJs() }
            });
            
            init.Add(addMain);
            
            if (options.Signable)
            {
                var addComp = new JsFunctionCall { Function = "this.addComponent" };
                var signParams = new { 
                    __inline = true, 
                    controllerName = "DocumentDigSignature", 
                    controllerAction = "SignList" 
                }.ToJs();

                addComp.Params.Add(new JsScalar { Value = "'signatureWindow'" });
                addComp.Params.Add(new JsInstance { Function = "MosKs.SignatureEntity.Page", Params = new List<JsProperty> { signParams } });

                init.Add(addComp);
            }

            if (!options.View.EditingDisabled)
            {
                var editParams = new JsObject();

                if (options.Stateful)
                { 
                    editParams.AddScalar("entityTypeId", "this.entityTypeId");
                }
                
                var addEdit = new JsFunctionCall { Function = "this.addComponent" };
                addEdit.Params.Add(new JsScalar() { Value = "'editWindow'" });
                addEdit.Params.Add(new JsInstance()
                {
                    Function = "ns.EditWindow",
                    Params = new List<JsProperty> { editParams },
                    Inline = !editParams.Properties.Any()
                });
                init.Add(addEdit);
                init.Add("this.components.editWindow.on('signButtonClick', this.onSignButtonClick, this);");
            }
            init.Add("");


            if (options.Stateful)
            {
                var pageGridStatePlugin = new JsFunctionCall { Function = "this.addPlugin" };

                pageGridStatePlugin.Params = new List<JsProperty>{ new JsInstance{ 
                    Inline = false,
                    Function = "EAS.States.PageGridStatePlugin", 
                    Params = new List<JsProperty>{ new {
                        gridName = "grid",
                        controllerName = "this.controllerName",
                        entityTypeId = "this.entityTypeId"
                    }.ToJs() }
                }};
                init.Add(pageGridStatePlugin);
            }

            if (options.View.DynamicFilter)
            { 
                var addPlugin = new JsFunctionCall { Function = "this.addPlugin" };
                
                addPlugin.Params.Add(new JsInstance()
                {
                    Inline = false,
                    Function = "MosKs.Plugin.QueryBuilder",
                    Params = new List<JsProperty>{ new {
                        gridName = "grid",
                        queryBuilderName = "queryBuilder",
                        entityName = options.ClassName
                    }.ToJs() }
                });
                init.Add(addPlugin);
            }

            if (!options.View.EditingDisabled)
            {
                var addPlugin = new JsFunctionCall { Function = "this.addPlugin" };

                addPlugin.Params.Add(new JsInstance()
                {
                    Inline = false,
                    Function = "EAS.PageGridEditWindowPlugin",
                    Params = new List<JsProperty>{ new {
                        controllerName = "this.controllerName",
                        gridName = "grid",
                        windowName = "editWindow"
                    }.ToJs() }
                });
                init.Add(addPlugin);
            }

            if (options.Permission != null)
            {
                var addPermission = new JsFunctionCall { Function = "this.addPlugin" };
                var pluginParams = (JsObject)new
                {
                    permissionsNamespace = "this.permissionsNamespace",
                    gridName = "grid"
                }.ToJs();
                
                addPermission.Params.Add(new JsInstance()
                {
                    Inline = false,
                    Function = "EAS.Permissions.GenericDictionaryPermissionsPlugin",
                    Params = new List<JsProperty>{ pluginParams }
                });

                if (!options.View.EditingDisabled)
                    pluginParams.Properties.Add(new JsScalar { Name = "windowName", Value = "'editWindow'" });

                init.Add(addPermission);
            }
            extendParams.Properties.Add(init);

            if (options.Signable)
            {
                var signHandler = new JsFunction { Name = "onSignButtonClick", Params = "" };
                signHandler.Body = new List<object>{
                    "this.components.signatureWindow.grid = this.components.grid;",
                    "this.components.signatureWindow.editWnd = this.components.editWindow;",
                    "this.components.signatureWindow.show(this.components.editWindow.objectId.value, this.entityTypeId, 'Подпись {0}');".F(options.DisplayName)
                };
                extendParams.Properties.Add(signHandler);
            }

            var pageExtend = new JsFunctionCall() { Name = "ns.Page", Function = "Ext3.extend" };
            pageExtend.Params.Add(new JsScalar() { Value = "EAS.Page" });
            pageExtend.Params.Add(extendParams);
            return pageExtend;
        }

        private JsScalar lc(string localString, string name = "")
        { 
            return new JsScalar{ Name = name, Value = "lc('{0}')".F(localString) };
        }
    }
}
