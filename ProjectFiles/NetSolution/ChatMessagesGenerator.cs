#region Using directives
using System;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using UAManagedCore;
using FTOptix.WebUI;
#endregion

public class ChatMessagesGenerator : BaseNetLogic
{
    private PeriodicTask messagesGeneratorTask;

    public override void Start()
    {
        // Gửi tin đầu tiên ngay lập tức
        GenerateSingleMessage();

        // Các tin tiếp theo cách nhau 2 giây (2000ms)
        messagesGeneratorTask = new PeriodicTask(GenerateSingleMessage, 2000, LogicObject);
        messagesGeneratorTask.Start();
    }

    public override void Stop()
    {
        messagesGeneratorTask?.Dispose();
    }

    private void GenerateSingleMessage()
    {
        ColumnLayout dstContainer = Owner.Get<ColumnLayout>("VerticalLayout1");
        if (dstContainer.Children.Count < 10)
        {
            var newMessage = InformationModel.Make<Chat>(Convert.ToString(DateTime.Now.Millisecond));
            string randString = LoremNET.Lorem.Sentence(2, 3);
            newMessage.Get<Label>("TopBar/Title").Text = randString.Substring(0, randString.Length - 1);
            newMessage.Get<Label>("Content").Text = LoremNET.Lorem.Sentence(6, 12);
            dstContainer.Add(newMessage);
        }
    }
}
