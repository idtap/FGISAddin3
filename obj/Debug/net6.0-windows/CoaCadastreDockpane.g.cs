﻿#pragma checksum "..\..\..\CoaCadastreDockpane.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "B0628ACCC0CCE7F68B6667BAA17698D69C3826EA"
//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

using ArcGIS.Desktop.Extensions;
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
    /// CoaCadastreDockpaneView
    /// </summary>
    public partial class CoaCadastreDockpaneView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 45 "..\..\..\CoaCadastreDockpane.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbCounty;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\..\CoaCadastreDockpane.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbTown;
        
        #line default
        #line hidden
        
        
        #line 57 "..\..\..\CoaCadastreDockpane.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbSec;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\..\CoaCadastreDockpane.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnLocateCounty;
        
        #line default
        #line hidden
        
        
        #line 70 "..\..\..\CoaCadastreDockpane.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnLocateTown;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\..\CoaCadastreDockpane.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnLocateSec;
        
        #line default
        #line hidden
        
        
        #line 101 "..\..\..\CoaCadastreDockpane.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtLandNo;
        
        #line default
        #line hidden
        
        
        #line 103 "..\..\..\CoaCadastreDockpane.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtSubNo;
        
        #line default
        #line hidden
        
        
        #line 105 "..\..\..\CoaCadastreDockpane.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtAddress;
        
        #line default
        #line hidden
        
        
        #line 114 "..\..\..\CoaCadastreDockpane.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox lstCadastre;
        
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
            System.Uri resourceLocater = new System.Uri("/FGISAddin3;component/coacadastredockpane.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\CoaCadastreDockpane.xaml"
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
            this.cmbCounty = ((System.Windows.Controls.ComboBox)(target));
            
            #line 49 "..\..\..\CoaCadastreDockpane.xaml"
            this.cmbCounty.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cmbCounty_SelectionChangedAsync);
            
            #line default
            #line hidden
            return;
            case 2:
            this.cmbTown = ((System.Windows.Controls.ComboBox)(target));
            
            #line 55 "..\..\..\CoaCadastreDockpane.xaml"
            this.cmbTown.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cmbTown_SelectionChangedAsync);
            
            #line default
            #line hidden
            return;
            case 3:
            this.cmbSec = ((System.Windows.Controls.ComboBox)(target));
            
            #line 61 "..\..\..\CoaCadastreDockpane.xaml"
            this.cmbSec.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent, new System.Windows.Controls.TextChangedEventHandler(this.cmbSec_TextChanged));
            
            #line default
            #line hidden
            
            #line 62 "..\..\..\CoaCadastreDockpane.xaml"
            this.cmbSec.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cmbSec_SelectionChangedAsync);
            
            #line default
            #line hidden
            return;
            case 4:
            this.btnLocateCounty = ((System.Windows.Controls.Button)(target));
            
            #line 67 "..\..\..\CoaCadastreDockpane.xaml"
            this.btnLocateCounty.Click += new System.Windows.RoutedEventHandler(this.btnLocateCounty_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btnLocateTown = ((System.Windows.Controls.Button)(target));
            
            #line 72 "..\..\..\CoaCadastreDockpane.xaml"
            this.btnLocateTown.Click += new System.Windows.RoutedEventHandler(this.btnLocateTown_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.btnLocateSec = ((System.Windows.Controls.Button)(target));
            
            #line 77 "..\..\..\CoaCadastreDockpane.xaml"
            this.btnLocateSec.Click += new System.Windows.RoutedEventHandler(this.btnLocateSec_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.txtLandNo = ((System.Windows.Controls.TextBox)(target));
            
            #line 101 "..\..\..\CoaCadastreDockpane.xaml"
            this.txtLandNo.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.txtLandNo_TextChanged);
            
            #line default
            #line hidden
            return;
            case 8:
            this.txtSubNo = ((System.Windows.Controls.TextBox)(target));
            
            #line 103 "..\..\..\CoaCadastreDockpane.xaml"
            this.txtSubNo.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.txtLandNo_TextChanged);
            
            #line default
            #line hidden
            return;
            case 9:
            this.txtAddress = ((System.Windows.Controls.TextBox)(target));
            return;
            case 10:
            this.lstCadastre = ((System.Windows.Controls.ListBox)(target));
            
            #line 116 "..\..\..\CoaCadastreDockpane.xaml"
            this.lstCadastre.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.LstCadastre_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

