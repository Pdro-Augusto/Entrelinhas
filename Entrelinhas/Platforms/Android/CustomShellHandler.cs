using Android.Content;
using Android.Content.Res;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;

namespace Entrelinhas.Platforms.Android
{
    public class CustomShellHandler : ShellRenderer
    {
        public CustomShellHandler(Context context) : base(context) { }

        protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        {
            return new CustomBottomNavViewAppearanceTracker(this, shellItem);
        }
    }

    public class CustomBottomNavViewAppearanceTracker : ShellBottomNavViewAppearanceTracker
    {
        public CustomBottomNavViewAppearanceTracker(IShellContext shellContext, ShellItem shellItem)
            : base(shellContext, shellItem) { }

        public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {
            base.SetAppearance(bottomView, appearance);

            // 1. Remove o título (e o espaço reservado para ele)
            bottomView.LabelVisibilityMode = LabelVisibilityMode.LabelVisibilityUnlabeled;

            // 2. Remove o ripple effect (efeito de onda ao tocar)
            bottomView.ItemRippleColor = ColorStateList.ValueOf(global::Android.Graphics.Color.Transparent);

            bottomView.ItemBackground = null; // Remove o efeito
        }
    }
}
