   M S B A M L   `   `   `  ����a  ]ArcGIS.Desktop.Extensions, Version=13.1.0.0, Culture=neutral, PublicKeyToken=8fc3cc631e44ad86hJclr-namespace:ArcGIS.Desktop.Extensions;assembly=ArcGIS.Desktop.ExtensionsArcGIS.Desktop.Extensions  5       
FGISAddin3*   $FGISAddin3.ImageMapQueryDockpaneView   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      a
extensionsJclr-namespace:ArcGIS.Desktop.Extensions;assembly=ArcGIS.Desktop.Extensions     5      $	��288��5	   #   .��5
      �� $��4,4,4,-5q�6   .+�5   
   � .+�5      �� $��
7,20,0,564q�6   $
��Left=�6,   �� 6H   $�服務選取��6L   6[   �� 5      -   5          ��Name$  
cmbService��5       �� DisplayMemberPath$ ServiceName��5       �� SelectedValuePath$ ServiceName��5       �� 
IsEditable$
 True��5      $��65,14,0,560q�5      $��30��6"   $
��Left=�6.   $	��205��6I   6U   �� 5      $��過濾條件設定:��6   $��2,55,10,520q�6&   �� 5      $
��起:��6   $��86,88,164,0q�6!   $	��Top=�66   $��23��6N   $��30��6Z   �� 5      -   5         ��Name$ txtFilterFrom��5      $��30��5      $��
116,85,9,0q�6   $	��Top=�6-   �� 5      $
��訖:��6   $��85,123,165,454q�6!    J� RenderTransformOrigin$ -4.075,1.254&�69   $��30��6^   �� 5      -   5        $ txtFilterEnd��5      $��30��5      $��116,123,9,451q�6   �� 5      -   5         ��Name$ ImageQueryButton��5      $��查詢��6/   $��
7,186,10,0q�5      $��24��6%   $	��Top=�61   �� 5      $3��-查詢結果(註:雙按項目可定位地圖)��6   $��2,215,26,360q�60   �� 5      -   5         ��Name$ lstImageQuery��5      $ showStr��5       $ posStr��5!      $��5,245,10,179q�5#      $	��180��6(   �� 5%      -   5        $ ImageQueryClearButton��5%      $��清除��64   $��230,450,0,0q�5&   1   $��48��6F   $	��Top=�6Q   $��24��6i   $
��Left=�6u   �� 5(      $H��B(註:依服務設定的查詢欄名無條件請設成空白跳過)��6   $��0,151,0,424q�67   $��Center=�6L   $	��278��6i   �� 5)      -   5         ��Name$ lblFilterField��5)      $��
[OBJECTID]��6,   $
��Left=�6A   $��8,89,0,0q�6\   $	��Top=�6n   $��78��6�   5*      5+      5,      