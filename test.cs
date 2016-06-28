public class SamsumgGamePadGP20
    {
        //Data for BluetoothLE conection.
        public const ushort vendorId = 0x04E8; //Samsung
        public const ushort productId = 0xA000; //GP20
        public const ushort usagePage = 0x0001; //Generic
        public const ushort usageId = 0x0005; //GamePads

        //Data
        private byte[] data;

        //Buttons
        public struct Stick
        {
            public byte x; //X Axis - 0=Left 255=Right
            public byte y; //Y Axis - 0=Up 255=Down
        }
        private Stick Stick1;
        private Stick Stick2; 
        private bool Button1 { get; set; }
        private bool Button2 { get; set; }
        private bool Button3 { get; set; }
        private bool Button4 { get; set; }
        private bool ButtonR { get; set; }
        private bool ButtonL { get; set; }
        private bool ButtonPlay { get; set; }
        private bool ButtonSelect { get; set; }
        private bool ButtonStart { get; set; }
        private byte Directional { get; set; }

        //Events Handle
        public delegate void ChangedEventHandler(object sender);
        public event ChangedEventHandler Update; //When Controler send info

        //Events for every button change
        public event ChangedEventHandler UpdateStick1;
        public event ChangedEventHandler UpdateStick2; 
        public event ChangedEventHandler UpdateButton1; 
        public event ChangedEventHandler UpdateButton2; 
        public event ChangedEventHandler UpdateButton3; 
        public event ChangedEventHandler UpdateButton4;
        public event ChangedEventHandler UpdateButtonR;
        public event ChangedEventHandler UpdateButtonL;
        public event ChangedEventHandler UpdateButtonPlay;
        public event ChangedEventHandler UpdateButtonSelect;
        public event ChangedEventHandler UpdateButtonStart;
        public event ChangedEventHandler UpdateButtonDirectional;

        //Constructor for defining the defalt values
        public SamsumgGamePadGP20()
        {
            this.Stick1.x = 128;
            this.Stick1.y = 128;
            this.Stick2.x = 128;
            this.Stick2.y = 128;
            this.Button1 = false;
            this.Button2 = false;
            this.Button3 = false;
            this.Button4 = false;
            this.ButtonR = false;
            this.ButtonL = false;
            this.ButtonPlay = false;
            this.ButtonSelect = false;
            this.ButtonStart = false;
            this.Directional = 8;
            Connect();

        }

        public async void Connect()
        {
            string Selector = HidDevice.GetDeviceSelector(usagePage, usageId, vendorId, productId);

            Debug.WriteLine("Finding Device.");
            var Devices = await DeviceInformation.FindAllAsync(Selector);
            Debug.WriteLine("Find:" + Devices.Count);

            if (Devices.Count > 0)
            {
                HidDevice device = await HidDevice.FromIdAsync(Devices.ElementAt(0).Id, Windows.Storage.FileAccessMode.ReadWrite);
                Debug.WriteLine(device.UsagePage);
                TypedEventHandler<HidDevice, HidInputReportReceivedEventArgs> input = new TypedEventHandler<HidDevice, HidInputReportReceivedEventArgs>(this.OnInputReportEvent);
                device.InputReportReceived += input;
            }
            else
            {
                Debug.WriteLine("No devices Found");
            }
        }

        public void UpdateData(Windows.Storage.Streams.IBuffer buffer)
        {
            this.data = buffer.ToArray();
            ParseData();
        }

        private void ParseData()
        {
            //Patern for this GamePad: 8 Bytes Length
            //Byte 0: |12345678| - Unknown, seens like a header
            //Byte 1: |12345678| - Stick 1 x axis. 0=Left 255=Right
            //Byte 2: |12345678| - Stick 1 y axis. 0=Up 255=Down
            //Byte 3: |12345678| - Stick 2 x axis. 0=Left 255=Right
            //Byte 4: |12345678| - Stick 2 y axis. 0=Up 255=Down
            //Byte 5: |12345678| - 1234-> Unknown | 5678 -> Directional, 0 to 8, Defalt 8, Clockwise, top = 0, top/rigth = 1, rigth = 2...top/left=7
            //Byte 6: |12345678| - 1->Right Button, 2->Left Button, 3->Unknown, 4->Button 4, 5->Button 3, 6->Unknown, 7->Button 2, 8->Button 1
            //Byte 7: |12345678| - 1->Play Button, 234->Unknown, 5->Start Button, 6->Select Button

            EventArgs e = new EventArgs();

            //Sticks are the full bytes
            if (this.Stick1.x != this.data[1]) { 
                this.Stick1.x = this.data[1];
                UpdateStick1?.Invoke(this);
            }
            if (this.Stick1.y != this.data[2])
            {
                this.Stick1.y = this.data[2];
                UpdateStick1?.Invoke(this);
            }
            if (this.Stick2.x != this.data[3])
            {
                this.Stick2.x = this.data[3];
                UpdateStick2?.Invoke(this);
            }
            if (this.Stick2.y != this.data[4])
            {
                this.Stick2.y = this.data[4];
                UpdateStick2?.Invoke(this);
            }

            //Get only the last 4 bits, data & 00001111
            byte NewDirectional = (byte)(this.data[5] & 15);
            if (this.Directional != NewDirectional)
            {
                this.Directional = NewDirectional;
                UpdateButtonDirectional?.Invoke(this);
            }
            //Getting the Buttons
            BitArray Byte67 = new BitArray(this.data.Skip(6).ToArray());
            if (this.Button1 != Byte67[0])
            {
                this.Button1 = Byte67[0];
                UpdateButton1?.Invoke(this);
            }

            if (this.Button2 != Byte67[1])
            {
                this.Button2 = Byte67[1];
                UpdateButton2?.Invoke(this);
            }

            if (this.Button3 != Byte67[3])
            {
                this.Button3 = Byte67[3];
                UpdateButton3?.Invoke(this);
            }

            if (this.Button4 != Byte67[4])
            {
                this.Button4 = Byte67[4];
                UpdateButton4?.Invoke(this);
            }

            if (this.ButtonR != Byte67[6])
            {
                this.ButtonR = Byte67[6];
                UpdateButtonR?.Invoke(this);
            }

            if (this.ButtonL != Byte67[7])
            {
                this.ButtonL = Byte67[7];
                UpdateButtonL?.Invoke(this);
            }

            if (this.ButtonPlay != Byte67[15])
            {
                this.ButtonPlay = Byte67[15];
                UpdateButtonPlay?.Invoke(this);
            }

            if (this.ButtonSelect != Byte67[10])
            {
                this.ButtonSelect = Byte67[10];
                UpdateButtonSelect?.Invoke(this);
            }

            if (this.ButtonStart != Byte67[11])
            {
                this.ButtonStart = Byte67[11];
                UpdateButtonStart?.Invoke(this);
            }

        }

        private void OnInputReportEvent(HidDevice sender, HidInputReportReceivedEventArgs e)
        {
            this.UpdateData(((HidInputReport)e.Report).Data);
        }

        public void ControlDebug()
        {
            Debug.WriteLine("Control: S1-x:" + this.Stick1.x + " S1-y:" + this.Stick1.y + " S2-x:" + this.Stick2.x + " S2.y" + this.Stick2.y +
                " Dire:" + this.Directional + " B1:" + this.Button1 + " B2:" + this.Button2 + " B3:" + this.Button3 + " B4:" + this.Button4 + " R:" + this.ButtonR +
                " L:" + this.ButtonL + " BPlay:" + this.ButtonPlay + " BSelect:" + this.ButtonSelect + " BStart:" + this.ButtonStart);
        }
        public void RawDebug()
        {
            Debug.Write("Data:");
            for (int i = 0; i < data.Length; i++)
            {
                Debug.Write(Convert.ToString(data[i], 2).PadLeft(8, '0') + "|");
            }
            Debug.WriteLine("");
        }
    }
