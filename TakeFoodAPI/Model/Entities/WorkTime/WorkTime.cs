﻿namespace TakeFoodAPI.Model.Entities.WorkTime;

public class WorkTime:ModelMongoDB
{
    public string Storeid { get; set; }
    public DateTime EndDay { get; set; }
    public DateTime StartDay { get; set; }
    public int OpenHour { get; set; }
    public int CloseHour { get; set; }
}
