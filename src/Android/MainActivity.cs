﻿using System;
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Bit.App.Abstractions;
using XLabs.Ioc;
using Plugin.Fingerprint.Abstractions;
using Plugin.Settings.Abstractions;
using Plugin.Connectivity.Abstractions;
using Acr.UserDialogs;
using Android.Content;
using System.Reflection;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Bit.Android
{
    [Activity(Label = "bitwarden",
        Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        WindowSoftInputMode = SoftInput.StateHidden)]
    public class MainActivity : FormsAppCompatActivity
    {
        private const string HockeyAppId = "d3834185b4a643479047b86c65293d42";

        protected override void OnCreate(Bundle bundle)
        {
            var policy = new StrictMode.ThreadPolicy.Builder().PermitAll().Build();
            StrictMode.SetThreadPolicy(policy);

            ToolbarResource = Resource.Layout.toolbar;
            TabLayoutResource = Resource.Layout.tabs;

            base.OnCreate(bundle);

            // workaround for app compat bug
            // ref https://forums.xamarin.com/discussion/62414/app-resuming-results-in-crash-with-formsappcompatactivity
            Task.Delay(10).Wait();

            Console.WriteLine("A OnCreate");
            Window.SetSoftInputMode(SoftInput.StateHidden);
            Window.AddFlags(WindowManagerFlags.Secure);

            var appIdService = Resolver.Resolve<IAppIdService>();
            var authService = Resolver.Resolve<IAuthService>();

            HockeyApp.Android.CrashManager.Register(this, HockeyAppId,
                new HockeyAppCrashManagerListener(appIdService, authService));

            Forms.Init(this, bundle);

            typeof(Color).GetProperty("Accent", BindingFlags.Public | BindingFlags.Static)
                .SetValue(null, Color.FromHex("d2d6de"));


            LoadApplication(new App.App(
                Resolver.Resolve<IAuthService>(),
                Resolver.Resolve<IConnectivity>(),
                Resolver.Resolve<IUserDialogs>(),
                Resolver.Resolve<IDatabaseService>(),
                Resolver.Resolve<ISyncService>(),
                Resolver.Resolve<IFingerprint>(),
                Resolver.Resolve<ISettings>(),
                Resolver.Resolve<ILockService>(),
                Resolver.Resolve<IGoogleAnalyticsService>(),
                Resolver.Resolve<ILocalizeService>()));

            MessagingCenter.Subscribe<Xamarin.Forms.Application>(Xamarin.Forms.Application.Current, "RateApp", (sender) =>
            {
                RateApp();
            });
        }

        protected override void OnPause()
        {
            Console.WriteLine("A OnPause");
            base.OnPause();
        }

        protected override void OnDestroy()
        {
            Console.WriteLine("A OnDestroy");
            base.OnDestroy();
        }

        protected override void OnRestart()
        {
            Console.WriteLine("A OnRestart");
            base.OnRestart();
        }

        protected override void OnStart()
        {
            Console.WriteLine("A OnStart");
            base.OnStart();
        }

        protected override void OnStop()
        {
            Console.WriteLine("A OnStop");
            base.OnStop();
        }

        protected override void OnResume()
        {
            Console.WriteLine("A OnResume");
            base.OnResume();
        }

        public void RateApp()
        {
            try
            {
                var rateIntent = RateIntentForUrl("market://details");
                StartActivity(rateIntent);
            }
            catch(ActivityNotFoundException)
            {
                var rateIntent = RateIntentForUrl("https://play.google.com/store/apps/details");
                StartActivity(rateIntent);
            }
        }

        private Intent RateIntentForUrl(string url)
        {
            var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse($"{url}?id={PackageName}"));
            var flags = ActivityFlags.NoHistory | ActivityFlags.MultipleTask;
            if((int)Build.VERSION.SdkInt >= 21)
            {
                flags |= ActivityFlags.NewDocument;
            }
            else
            {
                // noinspection deprecation
                flags |= ActivityFlags.ClearWhenTaskReset;
            }

            intent.AddFlags(flags);
            return intent;
        }
    }
}
