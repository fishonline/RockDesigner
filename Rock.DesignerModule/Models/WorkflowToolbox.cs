using Rock.ActivityDesignerLibrary;
using System;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Toolbox;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Activities;
using System.Text;

namespace Rock.DesignerModule.Models
{
    public class WorkflowToolbox
    {
        public static void LoadSystemIcon()
        {
            //AttributeTableBuilder builder = new AttributeTableBuilder();

            //string str = System.Environment.CurrentDirectory + @"\System.Activities.dll";
            //Assembly sourceAssembly = Assembly.LoadFile(str);

            //System.Resources.ResourceReader resourceReader = new System.Resources.ResourceReader(sourceAssembly.GetManifestResourceStream("System.Activities.Resources.resources"));

            //Type[] types = typeof(System.Activities.Activity).Assembly.GetTypes();

            //foreach (Type type in types)
            //{
            //    if (type.Namespace == "System.Activities.Statements")
            //    {
            //        CreateImageToActivity(builder, resourceReader, type);
            //    }
            //}      



            //MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        public static ToolboxCategoryItems LoadToolbox()
        {
            LoadSystemIcon();

            ToolboxCategoryItems toolboxCategoryItems = new ToolboxCategoryItems();


            ToolboxCategory categoryControlFlow = new ToolboxCategory("控制流")
            {  
                new ToolboxItemWrapper(typeof(DoWhile),"DoWhile"),
                new ToolboxItemWrapper(typeof(ForEach<>),"ForEach<T>"),
                new ToolboxItemWrapper(typeof(If),"If"),
                new ToolboxItemWrapper(typeof(Parallel),"Parallel"),
                new ToolboxItemWrapper(typeof(ParallelForEach<>),"ParallelForEach<T>"),
                new ToolboxItemWrapper(typeof(Pick),"Pick"),
                new ToolboxItemWrapper(typeof(PickBranch),"PickBranch"),
                new ToolboxItemWrapper(typeof(Sequence),"Sequence"),
                new ToolboxItemWrapper(typeof(Switch<>),"Switch<T>"),
                new ToolboxItemWrapper(typeof(While),"While")

            };
            toolboxCategoryItems.Add(categoryControlFlow);


            ToolboxCategory categoryFlowChart = new ToolboxCategory("流程图")
            {   
                new ToolboxItemWrapper(typeof(Flowchart),"Flowchart"),
                new ToolboxItemWrapper(typeof(FlowDecision),"FlowDecision"),
                new ToolboxItemWrapper(typeof(FlowSwitch<>),"FlowSwitch<T>"),

                new ToolboxItemWrapper(typeof(FlowSwitch<string>),"FlowSwitch<string>"),
                new ToolboxItemWrapper(typeof(FlowSwitch<double>),"FlowSwitch<double>")

            };
            toolboxCategoryItems.Add(categoryFlowChart);
        

            ToolboxCategory categoryPrimitives = new ToolboxCategory("基元")
            {   
                new ToolboxItemWrapper(typeof(Assign),"Assign"),
                new ToolboxItemWrapper(typeof(Delay),"Delay"),
                new ToolboxItemWrapper(typeof(InvokeMethod),"InvokeMethod"),
                new ToolboxItemWrapper(typeof(WriteLine),"WriteLine")
            };
            toolboxCategoryItems.Add(categoryPrimitives);

            ToolboxCategory categoryCustomActivities = new ToolboxCategory("自定义活动")
            {   
                new ToolboxItemWrapper(typeof(Artificial),"人工活动"),
                new ToolboxItemWrapper(typeof(Terminator), "流程结束"),
                //new ToolboxItemWrapper(typeof(BackToActor), "退回到发起人"),
                //new ToolboxItemWrapper(typeof(Countersignature),"会签专用活动")
            };
            toolboxCategoryItems.Add(categoryCustomActivities);

            return toolboxCategoryItems;
        }

        //private static void CreateImageToActivity(AttributeTableBuilder builder, System.Resources.ResourceReader resourceReader, Type builtInActivityType)
        //{
        //    System.Drawing.Bitmap bitmap = GetImageFromResource(resourceReader, builtInActivityType.IsGenericType ? builtInActivityType.Name.Split('`')[0] : builtInActivityType.Name);
        //    if (bitmap != null)
        //    {
        //        Type tbaType = typeof(System.Drawing.ToolboxBitmapAttribute);
        //        Type imageType = typeof(System.Drawing.Image);
        //        ConstructorInfo constructor = tbaType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { imageType, imageType }, null);
        //        System.Drawing.ToolboxBitmapAttribute tba = constructor.Invoke(new object[] { bitmap, bitmap }) as System.Drawing.ToolboxBitmapAttribute;
        //        builder.AddCustomAttributes(builtInActivityType, tba);
        //    }
        //}

        //private static Bitmap GetImageFromResource(System.Resources.ResourceReader resourceReader, string bitmapName)
        //{
        //    if (bitmapName == "ReceiveAndSendReplyFactory")
        //        bitmapName = "ReceiveAndSendReply";
        //    if (bitmapName == "SendAndReceiveReplyFactory")
        //        bitmapName = "SendAndReceiveReply";
        //    System.Collections.IDictionaryEnumerator dictEnum = resourceReader.GetEnumerator();
        //    System.Drawing.Bitmap bitmap = null;
        //    while (dictEnum.MoveNext())
        //    {
        //        if (String.Equals(dictEnum.Key, bitmapName))
        //        {
        //            bitmap = dictEnum.Value as System.Drawing.Bitmap;
        //            System.Drawing.Color pixel = bitmap.GetPixel(bitmap.Width - 1, 0);
        //            bitmap.MakeTransparent(pixel);
        //            break;
        //        }
        //    }

        //    return bitmap;
        //}

    }
}
