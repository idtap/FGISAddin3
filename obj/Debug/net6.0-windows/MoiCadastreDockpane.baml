   M S B A M L   `   `   `  ����a  ]ArcGIS.Desktop.Extensions, Version=13.1.0.0, Culture=neutral, PublicKeyToken=8fc3cc631e44ad86hJclr-namespace:ArcGIS.Desktop.Extensions;assembly=ArcGIS.Desktop.ExtensionsArcGIS.Desktop.Extensions  5       
FGISAddin3(   "FGISAddin3.MoiCadastreDockpaneView   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      a
extensionsJclr-namespace:ArcGIS.Desktop.Extensions;assembly=ArcGIS.Desktop.Extensions     5      $	��288��5	   #   .��5
      �� $
��Left=�6   $	��280��6,   $��4,4,4,4q�68   .+�5   
   �   � 	Resources  5       �� 
TargetType �� Property �� Value��%�   '��       '�� L     �� 5      " ��6   .�5      ��  � ��$ Center=�6:   5      �� 5      " ��6   .�5      ��  �@�Cursor$
 Hand��6/   5      5       � RowDefinitions 5      �� 5      $��40 �6    6.   �� 5      $��40 �6    6.   �� 5      $��40 �6    6.    � ColumnDefinitions 5      �� 5      $��60 �6#   60   �� 5      $��* �6#   6/   .+�5      �� $��2,0,0,0q�6   $
��Left=�6)   縣市/鄉鎮6K   �� 5      -   5         ��Name$ 	cmbCounty��5      $��1��6*    �� DisplayMemberPath$ 
countyname��5       �� SelectedValuePath$ 
countycode��5       	 �� 
IsEditable$
	 True��5!      $��90��5#      $��30��6*   $��1,5,0,5q�66   $
��Left=�6G   5$      �� 5%      $��1��6   $��10��6(   $��97,0,0,0q�63   $
��Left=�6E   /6d   �� 5&      -   5        $ cmbTown��5&      $��0��6(   $��1��65   $ townname��5'      $ towncode��5(      $
	 True��5)      $��90��5+      $��30��6*   $��	109,5,0,5q�66   $
��Left=�6I   5,      �� 5-      $��1��6   $��2,0,0,0q�6%   $
��Left=�66   	段小段6V   �� 5.      -   5        $ cmbSec��5.      $��1��6'   $��1��64   $ sectstr��5/      $ sectcode��50      $
	 True��51      $��0,4,0,6q�53      $��30��60   $
��Left=�6<   $	��200��6W   �� 54      $��2��6   $
��Left=�6%   $��2,0,0,0q�6@   地號6U   �� 55      -   5        
 ��Name$
 	txtLandNo��55      $��2��6)   $��1��66   $��30��57      $��0,5,0,5q�6"   $
��Left=�63   $��98��6N   $��00010000��6Y   58      �� 59   
   -   5         ��Name$ MoiQueryButton��59      $��查詢��6)   $��24��5:      $��52��6   $��Right=�6(   $��	0,10,20,0q�6D   5;      �� 5=   
   $3��-查詢結果(註:雙按項目可定位地圖)��6   $��2,10,0,0q�6,   $
��Left=�6>   �� 5>   
   -   5         ��Name$ lstMoiCadastre��5?      $	��164��5>      $	��258��6   $��5,5,0,0q�6+   $ showStr��5@      $ posStr��5A      $
��Left=�5C      �� 5E   
   -   5        $ MoiLstClearButton��5E      $��清除查詢結果��6,   $��24��5F      $��93��6   $��Right=�5G   -   $��	0,10,20,0q�6I   5I      5J      