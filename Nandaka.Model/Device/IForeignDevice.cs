namespace Nandaka.Model.Device
{
    public interface IForeignDevice : INandakaDevice
    {
        DeviceState State { get; set; }
    }
}