using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace WindowsDOC.Call
{
    public partial class HideWindowHelper
    {
        private readonly Window _window;
        private readonly Timer _timer;
        private readonly List<HideCore> _hideLogicList = new();

        // 获取鼠标全局位置
        public static class CursorHelper
        {
            [DllImport("user32.dll")]
            private static extern bool GetCursorPos(out POINT lpPoint);

            private struct POINT
            {
                public int X;
                public int Y;
            }

            public static Point GetCursorPosition()
            {
                GetCursorPos(out POINT p);
                return new Point(p.X, p.Y);
            }
        }

        private bool _isHide;
        private bool _isStarted;
        private HideCore? _lastHiderOn;
        private bool _isInAnimation;

        private HideWindowHelper(Window window)
        {
            _window = window;
            _timer = new Timer { Interval = 300 };
            _timer.Tick += Timer_Tick;
        }

        public HideWindowHelper AddHider<THideCore>() where THideCore : HideCore, new()
        {
            if (_isStarted) throw new Exception("调用了Start方法后无法在添加隐藏逻辑");
            var logic = new THideCore();
            logic.Init(_window, AnimationReport);
            _hideLogicList.Add(logic);
            return this;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_isInAnimation) return;
            if (_window.IsActive) return;

            Point cursorPosition = CursorHelper.GetCursorPosition(); //获取鼠标相对桌面的位置
            var isMouseEnter = cursorPosition.X >= _window.Left
                   && cursorPosition.X <= _window.Left + _window.Width + 30
                   && cursorPosition.Y >= _window.Top
                   && cursorPosition.Y <= _window.Top
                   + _window.Height + 30;

            //鼠标在里面
            if (isMouseEnter)
            {
                //没有隐藏，直接返回
                if (!_isHide) return;

                //理论上不会出现为null的情况
                if (_lastHiderOn != null)
                {
                    _window.Activate();

                    _window.ShowInTaskbar = true;
                    _lastHiderOn.Show();
                    _isHide = false;

                    return;
                }
            }


            foreach (var core in _hideLogicList)
            {
                //鼠标在里面并且没有隐藏
                if (isMouseEnter && !_isHide) return;

                //鼠标在里面并且当期是隐藏状态且当前处理器成功显示了窗体
                if (isMouseEnter && _isHide && core.Show())
                {
                    TryShow();
                }

                //鼠标在外面并且没有隐藏，那么调用当前处理器尝试隐藏窗体
                if (!isMouseEnter && !_isHide && core.Hide())
                {
                    _lastHiderOn = core;
                    _isHide = true;
                    _window.ShowInTaskbar = false;
                    return;
                }
            }
        }

        private void AnimationReport(bool isInAnimation)
        {
            _isInAnimation = isInAnimation;
        }

        public HideWindowHelper Start()
        {
            _isStarted = true;
            _timer.Start();
            return this;
        }

        public void Stop()
        {
            _timer.Stop();
            _isStarted = false;
        }

        public static HideWindowHelper CreateFor(Window window)
        {
            return new HideWindowHelper(window);
        }

        public void TryShow()
        {
            if (_lastHiderOn == null) return;
            _window.Activate();

            _window.ShowInTaskbar = true;
            _lastHiderOn.Show();
            _isHide = false;
        }
    }

    #region 隐藏逻辑基类

    public abstract class HideCore
    {
        private Action<bool> _animationStateReport = _ => { };

        internal void Init(Window window, Action<bool> animationStateReport)
        {
            WindowInstance = window;
            _animationStateReport = animationStateReport;
        }

        public abstract bool Show();

        public abstract bool Hide();

        protected Window WindowInstance { get; private set; } = new();

        protected void StartAnimation(DependencyProperty property, double from, double to)
        {
            _animationStateReport(true);
            var doubleAnimation = new DoubleAnimation(from, to, TimeSpan.FromSeconds(0.5));
            doubleAnimation.Completed += delegate
            {
                WindowInstance.BeginAnimation(property, null);
                _animationStateReport(false);
            };
            WindowInstance.BeginAnimation(property, doubleAnimation);
        }
    }

    #endregion

    #region 向上隐藏

    class HideOnTop : HideCore
    {
        public override bool Show()
        {
            if (WindowInstance.Top > 0) return false;
            StartAnimation(Window.TopProperty, WindowInstance.Top, 0);
            return true;
        }

        public override bool Hide()
        {
            if (WindowInstance.Top > 2) return false;
            if (WindowInstance.Width >= SystemParameters.WorkArea.Size.Width || WindowInstance.Height >= SystemParameters.WorkArea.Size.Height) return false;
            StartAnimation(Window.TopProperty, WindowInstance.Top, 0 - WindowInstance.Top - WindowInstance.Height + 2);
            return true;
        }
    }

    #endregion

    #region 向左隐藏

    class HideOnLeft : HideCore
    {
        public override bool Show()
        {
            if (WindowInstance.Left > 0) return false;
            StartAnimation(Window.LeftProperty, WindowInstance.Left, 0);
            return true;
        }

        public override bool Hide()
        {
            if (WindowInstance.Left > 2) return false;
            if (WindowInstance.Width >= SystemParameters.WorkArea.Size.Width || WindowInstance.Height >= SystemParameters.WorkArea.Size.Height) return false;
            StartAnimation(Window.LeftProperty, WindowInstance.Left, 0 - WindowInstance.Width + 2);
            return true;
        }
    }

    #endregion

    #region 向右隐藏

    class HideOnRight : HideCore
    {
        private readonly int _screenWidth;

        public HideOnRight()
        {
            foreach (var screen in Screen.AllScreens)
            {
                _screenWidth += screen.Bounds.Width;
            }
        }

        public override bool Show()
        {
            if (_screenWidth - WindowInstance.Left - WindowInstance.Width > 0) return false;
            StartAnimation(Window.LeftProperty, WindowInstance.Left, _screenWidth - WindowInstance.Width);
            return true;
        }

        public override bool Hide()
        {
            if (_screenWidth - WindowInstance.Left - WindowInstance.Width > 2) return false;
            if (WindowInstance.Width >= SystemParameters.WorkArea.Size.Width || WindowInstance.Height >= SystemParameters.WorkArea.Size.Height) return false;
            StartAnimation(Window.LeftProperty, WindowInstance.Left, _screenWidth - 2);
            return true;
        }
    }

    #endregion
}