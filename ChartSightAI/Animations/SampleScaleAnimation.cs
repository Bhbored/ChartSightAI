using CommunityToolkit.Maui.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.Animations
{
    partial class SampleScaleAnimation : BaseAnimation
    {
        public override async Task Animate(VisualElement view, CancellationToken token)
        {
            await view.ScaleTo(1.2, Length, Easing).WaitAsync(token);
            await view.ScaleTo(1, Length, Easing).WaitAsync(token);
        }
    }
}
