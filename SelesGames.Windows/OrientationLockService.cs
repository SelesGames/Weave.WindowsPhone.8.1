using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SelesGames.Phone
{
    public class OrientationLockService : DependencyObject
    {
        Frame frame;
        PhoneApplicationPageExtendedInfo currentPage;
        PageOrientation currentLockedOrientation;
        List<PhoneApplicationPageExtendedInfo> pageList = new List<PhoneApplicationPageExtendedInfo>();

        public OrientationLockService(Frame frame)
        {
            if (frame == null) throw new ArgumentNullException("frame cannot be null in constructor of OrientationLockService");

            this.frame = frame;
        }

        public void Start()
        {
            frame.Navigated += OnFrameNavigated;
        }

        public void Stop()
        {
            frame.Navigated -= OnFrameNavigated;
        }

        void OnFrameNavigated(object sender, NavigationEventArgs e)
        {
            var page = e.Content as Page;
            if (page == null)
                return;

            if (e.NavigationMode == NavigationMode.New)
                AddNewPage(page);
            else
            {
                var retrievalAttempt = pageList.FirstOrDefault(o => o.Page == page);
                if (retrievalAttempt == null)
                    AddNewPage(page);
                else
                    currentPage = retrievalAttempt;
            }

            if (IsLocked && CanCurrentPageSupportCurrentLockedOrientation)
                ApplyLock();
            else
                ApplyUnlock();
        }

        void AddNewPage(Page page)
        {
            currentPage = PhoneApplicationPageExtendedInfo.Create(page);
            pageList.Add(currentPage);
        }

        bool CanCurrentPageSupportCurrentLockedOrientation
        {
            get { return currentPage.OriginalSupportedOrientation.Contains(currentLockedOrientation); }
        }

        void OnIsLockedChanged(bool isLocked)
        {
            if (isLocked)
            {
                currentLockedOrientation = currentPage.GetPageOrientation();
                ApplyLock();
            }
            else
            {
                ApplyUnlock();
            }
        }

        void ApplyLock()
        {
            currentPage.SetSupportedOrientation(currentLockedOrientation.AsSupportedPageOrientation());
        }

        void ApplyUnlock()
        {
            currentPage.RestoreOriginalSupportedOrientation();
        }




        #region Dependency Properties




        #region IsLocked




        public bool IsLocked
        {
            get { return (bool)GetValue(IsLockedProperty); }
            set { SetValue(IsLockedProperty, value); }
        }

        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.Register(
                "IsLocked",
                typeof(bool),
                typeof(OrientationLockService),
                new PropertyMetadata(false, OnIsLockedPropertyChanged));

        static void OnIsLockedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as OrientationLockService;
            if (source != null && source.currentPage != null)
            {
                source.OnIsLockedChanged((bool)e.NewValue);
            }
        }




        #endregion




        #endregion
    }
}