﻿#pragma checksum "..\..\..\AutoAreaWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "F3F8E62A3770AEE9EFC5219F1C1D6CAB770CE293"
//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

using ArcGIS.Desktop.Framework.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace FGISAddin3 {
    
    
    /// <summary>
    /// AutoAreaWindow
    /// </summary>
    public partial class AutoAreaWindow : ArcGIS.Desktop.Framework.Controls.ProWindow, System.Windows.Markup.IComponentConnector {
        
        
        #line 27 "..\..\..\AutoAreaWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSelectPolygon;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\..\AutoAreaWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSelectVertex;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\AutoAreaWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox adjustArea;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\..\AutoAreaWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnAdjust;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\AutoAreaWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox nowArea;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\AutoAreaWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox areaFrom;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\..\AutoAreaWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox areaEnd;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.11.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/FGISAddin3;component/autoareawindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\AutoAreaWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.11.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.btnSelectPolygon = ((System.Windows.Controls.Button)(target));
            
            #line 27 "..\..\..\AutoAreaWindow.xaml"
            this.btnSelectPolygon.Click += new System.Windows.RoutedEventHandler(this.btnSelectPolygon_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.btnSelectVertex = ((System.Windows.Controls.Button)(target));
            
            #line 29 "..\..\..\AutoAreaWindow.xaml"
            this.btnSelectVertex.Click += new System.Windows.RoutedEventHandler(this.btnSelectVertex_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.adjustArea = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.btnAdjust = ((System.Windows.Controls.Button)(target));
            
            #line 36 "..\..\..\AutoAreaWindow.xaml"
            this.btnAdjust.Click += new System.Windows.RoutedEventHandler(this.btnAdjust_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.nowArea = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.areaFrom = ((System.Windows.Controls.TextBox)(target));
            return;
            case 7:
            this.areaEnd = ((System.Windows.Controls.TextBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

