﻿
using System;
using System.Windows;
using System.Windows.Media;

namespace HuntingDog.DogFace.Items
{
    public class Item : DependencyObject
    {
        public HuntingDog.DogEngine.Entity Entity
        {
            get;
            set;
        }

        public String Name
        {
            get;
            set;
        }

        public String NavigationTooltip
        {
            get;
            set;
        }

        public String MainObjectTooltip
        {
            get;
            set;
        }

        public ImageSource Image
        {
            get;
            set;
        }

        public ImageSource Action1
        {
            get;
            set;
        }

        public String Action1Description
        {
            get;
            set;
        }

        public String Action1Tooltip
        {
            get;
            set;
        }

        public ImageSource Action2
        {
            get;
            set;
        }

        public String Action2Description
        {
            get;
            set;
        }

        public String Action2Tooltip
        {
            get;
            set;
        }

        public Visibility Action3Visibility
        {
            get;
            set;
        }

        public ImageSource Action3
        {
            get;
            set;
        }

        public String Action3Description
        {
            get;
            set;
        }

        public String Action3Tooltip
        {
            get;
            set;
        }

        public Visibility Action4Visibility
        {
            get;
            set;
        }

        public ImageSource Action4
        {
            get;
            set;
        }

        public String Action4Description
        {
            get;
            set;
        }

        public String Action4Tooltip
        {
            get;
            set;
        }

        public Boolean IsChecked
        {
            get
            {
                return (Boolean) GetValue(IsCheckedProperty);
            }

            set
            {
                SetValue(IsCheckedProperty, value);
            }
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(Boolean), typeof(Item));

        public Boolean IsMouseOver
        {
            get
            {
                return (Boolean) GetValue(IsMouseOverProperty);
            }

            set
            {
                SetValue(IsMouseOverProperty, value);
            }
        }

        public static readonly DependencyProperty IsMouseOverProperty = DependencyProperty.Register("IsMouseOver", typeof(Boolean), typeof(Item));
    }
}
