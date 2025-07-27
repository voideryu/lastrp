#region Using directives
using System;
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.ODBCStore;
using FTOptix.SQLiteStore;
using FTOptix.DataLogger;
using FTOptix.WebUI;
#endregion

public class RuntimeNetLogic1 : BaseNetLogic
{
    private ODBCStore dbStore;

    public override void Start()
    {
        ExportData();
    }
    public override void Stop()
    {

    }
    [ExportMethod]
    public void ExportData()
    {
        dbStore = Project.Current.Get<ODBCStore>("DataStores/ODBCDatabase1");
        var var1 = Project.Current.GetVariable("Model/dashboard var/OSE");
        var var2 = Project.Current.GetVariable("Model/dashboard var/REU");
        var var3 = Project.Current.GetVariable("Model/dashboard var/CER");
        if (dbStore == null)
        {
            Log.Error("RuntimeNetLogic1", "ODBCStore not found. Please check the path.");
            return;
        }
        else
        {
            Log.Info("RuntimeNetLogic1", "ODBCStore found: " + dbStore.BrowseName);
        }

        // Truy vấn 
        string query1 = "SELECT PowerP FROM Devices ORDER BY DeviceID DESC LIMIT 1";
        string query2 = "SELECT Voltage FROM Devices ORDER BY DeviceID DESC LIMIT 1";
        string query3 = "SELECT Current FROM Devices ORDER BY DeviceID DESC LIMIT 1";
        
        dbStore.Query(query1, out string[] header1, out object[,] resultSet1);
        dbStore.Query(query2, out string[] header2, out object[,] resultSet2);
        dbStore.Query(query3, out string[] header3, out object[,] resultSet3);

        if (resultSet1.GetLength(0) > 0 && resultSet2.GetLength(0) > 0 && resultSet3.GetLength(0) > 0)
        {
            object lastPowerP = resultSet1[0, 0];
            int lastPowerPInt = Convert.ToInt32(lastPowerP);
            object lastVoltage = resultSet2[0, 0];
            int lastVoltageInt = Convert.ToInt32(lastVoltage);
            object lastCurrent = resultSet3[0, 0];
            int lastCurrentInt = Convert.ToInt32(lastCurrent);
            var1.Value = lastPowerPInt;
            var2.Value = lastVoltageInt;
            var3.Value = lastCurrentInt;
            Log.Info("RuntimeNetLogic1", $"Last PowerP value: {lastPowerP}");
            Log.Info("RuntimeNetLogic1", $"Last Voltage value: {lastVoltage}");
            Log.Info("RuntimeNetLogic1", $"Last Current value: {lastCurrent}");
        }
        else
        {
            Log.Info("RuntimeNetLogic1", "No data found in Devices table.");
        }
    }

}

