using ChartSightAI.Services.Interfaces;
﻿using System.Web;
﻿
﻿namespace ChartSightAI
﻿{
﻿    public partial class App : Application
﻿    {
﻿        public App()
﻿        {
﻿            InitializeComponent();
﻿
﻿         
﻿        }
﻿
﻿        protected override Window CreateWindow(IActivationState? activationState)
﻿        {
﻿            return new Window(new AppShell());
﻿        }
﻿
﻿       
﻿    }
﻿}