using ChartSightAI.MVVM.Views;
using ChartSightAI.Services.Interfaces;
using Syncfusion.Licensing;
using System.Web;
﻿
﻿namespace ChartSightAI
﻿{
﻿    public partial class App : Application
﻿    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
﻿        {
﻿            InitializeComponent();
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZfcnRTRGBYUUN/V0ZWYEg=");
            _serviceProvider = serviceProvider;

        }

        protected override Window CreateWindow(IActivationState? activationState)
﻿        {
﻿            return new Window(new AppShell());
﻿        }
﻿
﻿       
﻿    }
﻿}