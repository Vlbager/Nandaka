using Nandaka.Model.Attributes;

namespace Example.MasterExample
{
    public struct TestDeviceTable
    {
        [ReadRequestRegister(0x00)]
        public Distance FrontSensorDistance;
        [ReadRequestRegister(0x04)]
        public Distance RightSensorDistance;

        [WriteRequestRegister(0x80)]
        public int LeftDriveSpeed;
        [WriteRequestRegister(0x84)]
        public int RightDriveSpeed;
        [WriteRequestRegister(0x88)]
        public bool IsPowered;
    }
}