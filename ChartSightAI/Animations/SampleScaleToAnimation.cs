using CommunityToolkit.Maui.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.Animations
{
    partial class SampleScaleToAnimation : BaseAnimation
    {
        public double Scale { get; set; }

        public override Task Animate(VisualElement view, CancellationToken token)
            => view.ScaleTo(Scale, Length, Easing).WaitAsync(token);
    }
}
