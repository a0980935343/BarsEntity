using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class ViewGenerator : BaseBarsGenerator
    {
        public override GeneratedFile Generate(Project project, EntityOptions options, GeneratedFragments fragments)
        {
            var file = base.Generate(project, options, fragments);

            ControllerOptions controllerOpts = options.Controller;
            if (controllerOpts == null)
                controllerOpts = new ControllerOptions { Name = options.ClassName };

            if (!options.View.Inline)
            {
                var aFunction = new JsFunction() { Inline = false };

                var ns = new JsFunctionCall() { Function = "Ext3.ns", Name = "var ns", Inline = true };
                ns.Params.Add(new JsScalar{ Value = options.View.Namespace.Q("'") });

                aFunction.Add(ns);
                aFunction.Add("");
                aFunction.Add(nsGrid(options, project));

                if (!options.View.EditingDisabled)
                {
                    aFunction.Add("");
                    aFunction.Add(nsEditWindow(options, project, controllerOpts.Name));
                }
                aFunction.Add("");
                aFunction.Add(nsPage(options, project, controllerOpts.Name));
                aFunction.Add("");
                aFunction.Add("return ns.Page;");

                var deps = new JsArray();

                if (options.Signable)
                    deps.AddString("modules/MosKs/DigitalSignature/MosKs.SignatureEntity");
                
                if (options.View.DynamicFilter)
                    deps.AddString("modules/MosKs.Plugin.QueryBuilder");

                if (options.View.TreeGrid)
                    deps.AddString("modules/MosKs.Controls.TreeGrid");

                if (options.Stateful)
                {
                    deps.AddString("modules/EAS.States.StateField");
                    deps.AddString("modules/EAS.States.StateColumn");
                    deps.AddString("modules/EAS.States.PageGridStatePlugin");
                    deps.AddString("modules/EAS.States.StateFilterCombobox");
                }

                var define = new JsFunctionCall { Function = "define" };
                define.Params.Add(deps);
                define.Params.Add(aFunction);

                file.Name = options.ClassName + ".js";
                file.Path = "Views\\" + (options.IsDictionary ? "Dict\\" : (!string.IsNullOrWhiteSpace(options.Subfolder) ? options.Subfolder : ""));
                file.Body = define.Draw(0);
                file.Properties.Add("BuildAction", 3);
                
                fragments.AddLines("ResourceManifest.cs", this, new List<string> { 
                    "container.Add(\"scripts/modules/{3}.js\", \"{0}.dll/{0}.Views.{2}{1}.js\");".F(project.Name, options.ClassName, options.IsDictionary ? "Dict." : "", options.View.Namespace)} );

                return file;
            }
            else
            {
                var ns = new NamespaceInfo();
                var cls = new ClassInfo();

                ns.Name = "{0}.ViewModels".F(project.Name);

                ns.InnerUsing.Add("Bars.B4");
                ns.InnerUsing.Add("Bars.B4.Modules.Templates");
                ns.InnerUsing.Add("{0}.Entities".F(project.Name));

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
                    "container.Add(\"scripts/modules/{0}.{1}.js\", new GridPageView<{1}ViewModel>());".F(project.Name, options.ClassName, options.IsDictionary ? "Dict." : "")});

                file.Name = options.ClassName + "ViewModel.cs";
                file.Path = "ViewModels\\" + (options.IsDictionary ? "Dict\\" : "");
                file.Body = ns.Generate();
                return file;
            }
        }

        private static JsFunctionCall nsGrid(EntityOptions options, Project project)
        {
            var gridConfig = new JsFunctionCall { Function = "Ext3.apply", Name = "return" };

            gridConfig.Params.Add(new JsObject() { Inline = true });
            gridConfig.Params.Add(new JsScalar() { Value = "config" });


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
            var storeParams = new JsObject();
            
            var storeFields = new JsArray() { Name = "fields", Inline = true };
            storeFields.AddString("Id");
            foreach (var field in options.Fields.Where(x => x.DisplayName != "" && !x.Collection && !x.ParentReference))
                storeFields.AddString(field.FieldName);

            if (options.Signable)
                storeFields.AddString("Signed");

            if (options.View.TreeGrid)
            {
                gridApply.AddString("master_column_id", "column" + options.Fields.First(x => x.DisplayName != "" && !x.Collection && !x.GroupField).FieldName);

                storeParams.AddBoolean("autoLoad", true);
                storeParams.AddBoolean("remoteSort", true);

                storeFields.AddString("_parent");
                storeFields.AddString("_is_leaf");
                storeFields.AddString("_is_loaded");

                storeParams.Add(new JsInstance
                {
                    Name = "reader",
                    Function = "Ext3.data.JsonReader",
                    Params = new List<JsProperty> { new JsObject{
                        Properties = new List<JsProperty>{
                            JsScalar.String("idProperty", "Id"),
                            JsScalar.String("root", "data"),
                            JsScalar.String("totalProperty", "totalCount"),
                            storeFields
                        }
                    }
                    }
                });

                storeParams.Add(new JsInstance
                {
                    Name = "proxy",
                    Function = "Ext3.data.HttpProxy",
                    Params = new List<JsProperty>{new JsObject
                    {
                        Inline = true,
                        Properties = new List<JsProperty>{
                            JsScalar.String( "method", "POST"),
                            JsScalar.New("url", "EAS.url.action('/' + config.controllerName + '/List/')"),
                            JsScalar.Boolean("json", true)
                        }
                    }}
                });

                storeParams.Add(new JsObject
                {
                    Name = "baseParams",
                    Properties = new List<JsProperty>{
                        JsScalar.Number("start", 0),
                        JsScalar.Number("limit", 20)}
                });
            }
            else
            {
                var groupField = options.Fields.FirstOrDefault(x => x.GroupField);
                if (groupField != null)
                {
                    storeParams.AddString("groupField", groupField.FieldName);

                    gridApply.Add(new JsInstance
                    {
                        Name = "view",
                        Function = "Ext3.grid.GroupingView",
                        Inline = true,
                        Params = new List<JsProperty>{ new JsObject{
                            Inline = true,
                            Properties = new List<JsProperty>{ JsScalar.Boolean("hideGroupedColumn", true) }
                        }}
                    });
                }
                storeParams.Add(storeFields);
                storeParams.AddScalar("controllerName", "config.controllerName");
            }

            store.Params.Add(storeParams);

            gridApply.Add(store);

            var columns = new JsArray() { Name = "columns" };

            if (options.View.SelectionModel.StartsWith("Checkbox"))
                columns.AddScalar("sm");

            columns.Values.Add(new JsInstance { Function = "EAS.GridEditColumn" });

            if (options.Signable)
            {
                var signColumn = new JsObject{ Inline = false };
                signColumn.AddString("xtype", "easgridactionscolumn");
                signColumn.AddString("id", "columnSign");
                signColumn.Add(new JsFunction { Name = "getRowActions", Params = "value, meta, record", Body = new List<object> { "return [{ name: 'SignData', iconCls: (record.data.Signed ? 'icon-signed' : '') }];" } });
                signColumn.AddLocal("header", "ЭЦП");

                columns.Values.Add(signColumn);
            }

            foreach (var field in options.Fields.Where(x => x.DisplayName != "" && !x.Collection && !x.ParentReference && !x.GroupField))
            {
                if (options.Stateful && field.FieldName == "State")
                {
                    var col = new JsInstance
                    {
                        Function = "EAS.States.StateColumn",
                        Inline = true,
                        Params = new List<JsProperty> {
                            new JsObject
                            {
                                Inline = true,
                                Properties = new List<JsProperty> { 
                                    JsScalar.String("dataIndex", "State"),
                                    JsScalar.Local("header", "Статус"),
                                    JsScalar.Number("width", 100),
                                    JsScalar.Boolean("fixed", true)
                                }
                            }
                        }
                    };
                    columns.Values.Add(col);
                }
                else
                {
                    var col = new JsObject() { Inline = true };
                    col.AddString("dataIndex", field.FieldName);
                    col.AddLocal("header", field.DisplayName);
                    col.AddString("id", "column"+field.FieldName);

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
                        col.AddString("xtype", field.ViewColumnType);
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
            foreach (var field in options.Fields.Where(x => x.DisplayName != "" && !x.Collection && !x.ParentReference && !x.GroupField))
            {
                var fil = new JsObject() { Name = field.FieldName, Inline = true };
                if (options.Stateful && field.FieldName == "State")
                {
                    fil.AddString("xtype", "statefiltercombobox");
                    fil.AddString("stateTypeId", options.ClassFullName);
                    fil.AddBoolean("editable", false);
                }
                else
                {
                    fil.AddString("xtype", field.TypeName != "bool" ? (field.ViewType == "easselectfield" ? "textfield" : field.ViewType) : "combobox");
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

        private static JsFunctionCall nsEditWindow(EntityOptions options, Project project, string controllerName)
        {
            var extendParams = new JsObject();

            extendParams.AddScalar("width", "600");
            extendParams.AddLocal("title", options.View.Title);
            extendParams.AddScalar("padding", "5");

            var init = new JsFunction() { Name = "initComponent" };
            init.Add("ns.EditWindow.superclass.initComponent.call(this);");
            init.Add("");
            init.Add("var thisWindow = this;");
            init.Add("this.addEvents('signButtonClick');");

            var signButton = new JsFunctionCall { Function = "this.saveButtonGroup.add" };
            var signParams = new JsObject();
            signParams.AddString("xtype", "button");
            signParams.AddLocal("text", "ЭЦП");
            signParams.AddString("iconCls", "icon-signed");
            signParams.AddString("ref", "signButton");
            signParams.Properties.Add(new JsObject { Name = "listeners", Properties = new List<JsProperty> { 
                new JsObject{ Name = "click", Properties = new List<JsProperty>{
                    new JsFunction{ Name = "fn", Params = "btn", Body = new List<object>{ "thisWindow.fireEvent('signButtonClick');"}},
                    new JsScalar{ Name = "scope", Value = "this"}
                }}
            } });
            signButton.Params.Add(signParams);
            init.Add(signButton);

            var formObject = new JsObject();
            formObject.AddString("xtype", "form");
            formObject.AddScalar("padding", "5");
            if (options.AcceptFiles)
                formObject.AddBoolean("fileUpload", true);

            var fields = new JsArray(){ Name = "items" };
            foreach (var field in options.Fields.Where(x => x.DisplayName != "" && !x.Collection))
            {
                var fld = new JsObject() { Inline = true };
                

                fld.AddString("xtype", field.ViewType);
                fld.AddLocal("fieldLabel", field.DisplayName);

                if (options.Stateful && field.FieldName == "State")
                {
                    fld.AddString("ref", "../field_State");
                    fld.AddString("controllerName", controllerName);
                    fld.AddScalar("entityTypeId", "this.entityTypeId");
                }
                else
                {
                    fld.AddString("dataIndex", field.FieldName);
                    
                    if (!field.Nullable)
                        fld.AddBoolean("allowBlank", false);
                }
                

                if (field.ViewType == "easselectfield")
                {
                    fld.AddString("idProperty", "Id");
                    fld.AddString("textProperty", field.TextProperty);
                        var selectConfig = new JsObject() { Name = "selectWindowConfig" };
                        selectConfig.AddString("title", field.DisplayName);
                        selectConfig.AddScalar("storeConfig", "{{ fields: ['Id', '{1}'], controllerName: '{0}' }}".F(field.TypeName, field.TextProperty));
                        selectConfig.AddScalar("gridConfig", "{{ columns: [{{ dataIndex: '{0}', header: lc('{1}'), xtype: 'easwraptextcolumn' }}] }}".F(field.TextProperty, field.DisplayName));
                    fld.Add(selectConfig);
                    fld.Inline = false;
                }
                fields.Values.Add(fld);
            }

            formObject.Add(fields);

            var add = new JsFunctionCall() { Function = "this.add" };
            add.Params.Add(formObject);

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

        private static JsFunctionCall nsPage(EntityOptions options, Project project, string controllerName)
        {
            var extendParams = new JsObject();

            extendParams.AddLocal("title", options.View.Title);
            extendParams.AddString("controllerName", controllerName);

            if (options.Permission != null)
                extendParams.AddString("permissionsNamespace", options.Permission.Prefix);

            if (options.Stateful || options.Signable)
                extendParams.AddString("entityTypeId", options.ClassFullName);
            
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
                Params = new List<JsProperty>{ 
                    new JsObject{ 
                        Properties = new List<JsProperty>{
                            new JsScalar{ Name = "controllerName", Value = "this.controllerName"}}}}
            });
            
            init.Add(addMain);

            

            if (options.Signable)
            {
                var addComp = new JsFunctionCall { Function = "this.addComponent" };
                var signParams = new JsObject() { Inline = true };
                signParams.AddString("controllerName", "DocumentDigSignature");
                signParams.AddString("controllerAction", "SignList");

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

                var pluginParams = new JsObject();
                pluginParams.AddString("gridName", "grid");
                pluginParams.AddScalar("controllerName", "this.controllerName");
                pluginParams.AddScalar("entityTypeId", "this.entityTypeId");
                
                pageGridStatePlugin.Params = new List<JsProperty>{ new JsInstance{ 
                    Inline = false,
                    Function = "EAS.States.PageGridStatePlugin", 
                    Params = new List<JsProperty>{ pluginParams } }
                };
                init.Add(pageGridStatePlugin);
            }

            if (options.View.DynamicFilter)
            { 
                var addPlugin = new JsFunctionCall { Function = "this.addPlugin" };
                var pluginParams = new JsObject();
                pluginParams.AddString("gridName", "grid");
                pluginParams.AddString("queryBuilderName", "queryBuilder");
                pluginParams.AddString("entityName", options.ClassName);

                addPlugin.Params.Add(new JsInstance()
                {
                    Inline = false,
                    Function = "MosKs.Plugin.QueryBuilder",
                    Params = new List<JsProperty>{ pluginParams }
                });
                init.Add(addPlugin);
            }

            if (!options.View.EditingDisabled)
            {
                var addPlugin = new JsFunctionCall { Function = "this.addPlugin" };
                var pluginParams = new JsObject();
                pluginParams.AddScalar("controllerName", "this.controllerName");
                pluginParams.AddString("gridName", "grid");
                pluginParams.AddString("windowName", "editWindow");

                addPlugin.Params.Add(new JsInstance()
                {
                    Inline = false,
                    Function = "EAS.PageGridEditWindowPlugin",
                    Params = new List<JsProperty>{ pluginParams }
                });

                init.Add(addPlugin);
            }

            if (options.Permission != null)
            {
                var addPermission = new JsFunctionCall { Function = "this.addPlugin" };
                var pluginParams = new JsObject();
                pluginParams.AddScalar("permissionsNamespace", "this.permissionsNamespace");
                pluginParams.AddString("gridName", "grid");

                addPermission.Params.Add(new JsInstance()
                {
                    Inline = false,
                    Function = "EAS.Permissions.GenericDictionaryPermissionsPlugin",
                    Params = new List<JsProperty>{ pluginParams }
                });

                if (!options.View.EditingDisabled)
                    ((JsObject)((JsInstance)addPermission.Params.First()).Params.First())
                        .Properties.Add(new JsScalar { Name = "windowName", Value = "'editWindow'" });

                init.Add(addPermission);
            }
            extendParams.Properties.Add(init);

            if (options.Signable)
            {
                var signHandler = new JsFunction { Name = "onSignButtonClick", Params = "" };
                signHandler.Body = new List<object>{
                    "this.components.signatureWindow.grid = this.components.grid",
                    "this.components.signatureWindow.editWnd = this.components.editWindow",
                    "this.components.signatureWindow.show(this.components.editWindow.objectId.value, this.entityTypeId, 'Подпись {0}')".F(options.DisplayName)
                };
                extendParams.Properties.Add(signHandler);
            }

            var pageExtend = new JsFunctionCall() { Name = "ns.Page", Function = "Ext3.extend" };
            pageExtend.Params.Add(new JsScalar() { Value = "EAS.Page" });
            pageExtend.Params.Add(extendParams);
            return pageExtend;
        }
    }
}
