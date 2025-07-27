#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using FTOptix.Store;
using FTOptix.SQLiteStore;
using FTOptix.DataLogger;
using FTOptix.WebUI;
#endregion

public class DataGridCreator : BaseNetLogic
{
    private LongRunningTask queryTask;

    public override void Start()
    {
        
        QueryOnStore("DataLogger1");
    }

    public override void Stop()
    {
        // Dọn dẹp khi dừng logic
        queryTask?.Dispose();
    }

    [ExportMethod]
    public void QueryOnStore(string tableName)
    {
        queryTask?.Dispose();
        var arguments = new object[] { tableName };
        queryTask = new LongRunningTask(QueryAndUpdate, arguments, LogicObject);
        queryTask.Start();
    }

    private void QueryAndUpdate(LongRunningTask myTask, object args)
    {
        var argumentsArray = (object[])args;
        var tableName = (string)argumentsArray[0];

        if (string.IsNullOrEmpty(tableName))
            return;

        var store = Project.Current.Get<Store>("DataStores/EmbeddedDatabase1");
        var dataGrid = Owner.Get<DataGrid>("DataGrid");

        if (store == null || dataGrid == null)
            return;

        // Reset lưới
        dataGrid.Query = "";

        var query = $"SELECT * FROM {tableName}";
        store.Query(query, out String[] header, out Object[,] resultSet);

        if (header == null || resultSet == null)
            return;

        // Xóa các cột cũ
        dataGrid.Columns.Clear();

        DynamicLink lastDynamicLink = null;

        // Tạo cột mới
        foreach (var columnName in header)
        {
            var newDataGridColumn = InformationModel.MakeObject<DataGridColumn>(columnName);
            newDataGridColumn.Title = columnName;
            newDataGridColumn.DataItemTemplate = InformationModel.MakeObject<DataGridLabelItemTemplate>("DataItemTemplate");

            var dynamicLink = InformationModel.MakeVariable<DynamicLink>("DynamicLink", FTOptix.Core.DataTypes.NodePath);
            dynamicLink.Value = "{Item}/" + NodePath.EscapeNodePathBrowseName(columnName);
            newDataGridColumn.DataItemTemplate.GetVariable("Text").Refs.AddReference(FTOptix.CoreBase.ReferenceTypes.HasDynamicLink, dynamicLink);
            newDataGridColumn.OrderBy = dynamicLink.Value;

            dataGrid.Columns.Add(newDataGridColumn);
            lastDynamicLink = dynamicLink;
        }

        // Thiết lập sắp xếp theo cột cuối cùng (không bắt buộc)
        dataGrid.SortColumnVariable.Refs.AddReference(FTOptix.CoreBase.ReferenceTypes.HasDynamicLink, lastDynamicLink);

        // Gán truy vấn có sắp xếp theo thời gian
        dataGrid.Query = query + " ORDER BY Timestamp DESC";
    }
}
