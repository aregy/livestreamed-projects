﻿using CommunityToolkit.Maui.Views;
using DevExpress.XtraSpreadsheet.Commands.Internal;
using Microsoft.Maui.Graphics.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SignPDF {
    public class DrawingViewMVVMBehavior : Behavior<DrawingView> {
        DrawingView drawingView;
        public static readonly BindableProperty ClearCommandProperty;
        public static readonly BindableProperty AcceptDrawingCommandProperty;
        public static readonly BindableProperty DrawingAcceptedCommandProperty = BindableProperty.Create("DrawingAcceptedCommand", typeof(ICommand), typeof(DrawingViewMVVMBehavior));
        internal static readonly BindablePropertyKey ClearCommandPropertyKey = BindableProperty.CreateReadOnly("ClearCommand", typeof(ICommand), typeof(DrawingViewMVVMBehavior), null);
        internal static readonly BindablePropertyKey AcceptDrawingCommandPropertyKey = BindableProperty.CreateReadOnly("AcceptDrawingCommand", typeof(ICommand), typeof(DrawingViewMVVMBehavior), null);
        static DrawingViewMVVMBehavior() {
            ClearCommandProperty = ClearCommandPropertyKey.BindableProperty;
            AcceptDrawingCommandProperty = AcceptDrawingCommandPropertyKey.BindableProperty;
        }
        public ICommand DrawingAcceptedCommand {
            get { return (ICommand)GetValue(DrawingAcceptedCommandProperty); }
            set { SetValue(DrawingAcceptedCommandProperty, value); }
        }
        public ICommand ClearCommand {
            get { return (ICommand)GetValue(ClearCommandProperty); }
            internal set { SetValue(ClearCommandPropertyKey, value); }
        }
        public ICommand AcceptDrawingCommand {
            get { return (ICommand)GetValue(AcceptDrawingCommandProperty); }
            internal set { SetValue(AcceptDrawingCommandPropertyKey, value); }
        }
        public DrawingViewMVVMBehavior() {
            ClearCommand = new Command(() => drawingView.Clear());
            AcceptDrawingCommand = new Command(AcceptDrawing);
        }
        public async void AcceptDrawing() {
            using Stream origJpgStream = await drawingView.GetImageStream(200, 200);
            origJpgStream.Seek(0, SeekOrigin.Begin);
            Microsoft.Maui.Graphics.IImage img = PlatformImage.FromStream(origJpgStream, ImageFormat.Jpeg);
            if (DrawingAcceptedCommand != null) {
                DrawingAcceptedCommand.Execute(img.AsBytes(ImageFormat.Png));
            }
        }
        protected override void OnAttachedTo(BindableObject bindable) {
            drawingView = (DrawingView)bindable;
            drawingView.BindingContextChanged += DrawingView_BindingContextChanged;
            base.OnAttachedTo(bindable);
        }
        protected override void OnDetachingFrom(BindableObject bindable) {
            drawingView.BindingContextChanged -= DrawingView_BindingContextChanged;
            base.OnDetachingFrom(bindable);
        }
        private void DrawingView_BindingContextChanged(object sender, EventArgs e) {
            this.BindingContext = drawingView.BindingContext;
        }
    }
}