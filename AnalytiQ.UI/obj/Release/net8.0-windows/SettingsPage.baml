   M S B A M L   `   `   `  ���� F  BAnalytiQ.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:AnalytiQ.UIAnalytiQ.UI  *    $AnalytiQ.UI.AnalytiQ.UI.SettingsPage   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     +localclr-namespace:AnalytiQ.UI       N� Title$  Settings�� � 	Resources     BooleanToVisibilityConverter  DarkBackgroundValue  CardBackgroundValue  ContentBackgroundValue  BorderValue  TextPrimaryValue  TextMutedValue  AccentValue  AccentHoverValue 	 AccentLightValue 
 WarningValue  DarkBackground �� Color  CardBackground  ContentBackground 
 Border  TextPrimary  	TextMuted 
 Accent  AccentHover  AccentLight  Warning  BackgroundGradient �� 
StartPoint �� EndPoint � Color � Offset  PrimaryButton �� 
TargetType �� Property	 �� Value
 �� 
TargetType ��Name �� CornerRadius �� Padding �� Triggers P� Property P� Value �� 
TargetName  SecondaryButton  
IconButton  Card3  -System.Windows.Media.Effects.DropShadowEffect   
BlurRadius   ShadowDepth   Opacity   	Direction   Color  SectionHeader  	FormLabel  	TextInput ��Name  ProductList  ProductListItem � ColumnDefinitions ��Name � ToolTip
 � Tag �� Value �� Binding �� RelativeSource �� Mode  �� AncestorType! �� Path  EmptyStateMessage��%�  &	        &	      &	      &	 '     &	 8     &	 I     &	 Z     &	 k     &	 |     &		 �     &	
 �     &	 �     7  &	 �     7  &	 �     7  &	 �     7  &	 �     7  &	 �     7  &	 �     7  &	 �     7  &	     7 	 &	     7 
 &	     &	 g    7  7  7  &	 �    7  7  7  &	 b    7  &	 �    7  7  &	 �    7  &	 [    7  &	 �    7  7  7  7  7  &	 �
    7  7  &	 _    7  7  7  &	 �    7  �� ��#1A1A2A����#22222F����#2A2A38����#383850����#FFFFFF����#BBBBCC����#3CBFAE����#4FD0BF����	#3CBFAE20����#FF5C78���� 8   �� 8   �� 8   �� 8   �� 8   �� 8   �� 8   �� 8   �� 8   �� 8   �� $	 0,0&�$	 0,1&�.��� $ #1A1A2A��$ 0Q�� $ #22222F��$ 1Q��� " ��.���  � ��8	   ��  � ��8	  ��  � ��$	 Medium"���  � ��$	 13(���  � ��$	 0q���  � ��$	 36����  � ��$
	 16,0q���  �@�Cursor$
	 Hand����  � ��	 �� "
 ��.R��� $ border��#��z��$ 6��# z��._��� $��Center=�$��Center=� P�  �@J�IsMouseOver$
 True��. ��� 8	  $ border�� � ��P�  � ��$
 True��. ��� $ border�� � ��	 �#2D9082P�  � {�$ False��. ��� $ border�� � ��	 �#444444��  � ��	 �#777777�� " ��.���  � ��
	 ���� ��  � ��8	   ��  � ��$	 Medium"���  � ��$	 13(���  � ��$	 1q���  � ��8	  ��  � ��$	 36����  � ��$
	 16,0q���  �@�Cursor$
	 Hand����  � ��	 �� "
 ��.R��� $ border��#��z��#��z��#��z��$ 6��# z��._��� $��Center=�$��Center=� P�  �@J�IsMouseOver$
 True��. ��� 8	  $ border�� � ��P�  � ��$
 True��. ��� $ border�� � ��	 �	#3CBFAE30�� " ��.���  � ��
	 ���� ��  � ��8	   ��  � ��$	 0q���  � ��$	 32����  � ��$	 32����  �@�Cursor$
	 Hand����  � ��	 �� "
 ��.R��� $ border��#��z��$ 4��$ 4q�._��� $��Center=�$��Center=� P�  �@J�IsMouseOver$
 True��. ��� $ border�� � ��	 �#383850P�  � ��$
 True��. ��� $ border�� � ��	 �#25253A�� " ��.���  � ��8	   ��  � ��8	  ��  � ��$	 1q���  �@��CornerRadius$	 10����  � ��$	 0,0,0,16q���  �@��Padding$	 20q���  �@J�Effect	   $ 10Q�$ 2Q�$
 0.25Q�$	 315Q�$ #121220���� " ��.���  � ��$	 18(���  � ��$	 SemiBold"���  � ��8	   ��  � ��$	 0,0,0,12q��� " ��.���  � ��$	 Center=���  � ��8	   ��  � ��$	 12(���  � ��$	 0,0,12,0q��� " ��.���  � ��$
	 12,6q���  � ��$	 1q���  � ��8	   ��  � ��8	  ��  � ��$	 13(���  � ��8	  ��  � ��$	 Center=���  � ��$	 38����  � ��	 �� "
 ��.R��� $ Border��#��z��#��z��#��z��$ 6��._��� $ PART_ContentHost��#��z�� P�  �@J�IsMouseOver$
 True��. ��� $ Border��8	   � ��P�  �@J�	IsFocused$
 True��. ��� $ Border��8	   � ���� $ Border�� � ��$		 1.5q��� " ��.���  � ��8	   ��  � ��8	  ��  � ��$	 1q���  � ��$	 0q���  � ��	 �� "
 ��.R��� #��z��#��z��#��z��$ 8��._��� #��z��$|�False��.���� �� " ��.���  � ��$	 16,10q���  � ��
	 ���� ��  � ��$	 0,0,0,1q���  � ��8	   ��  � ��	 �� "
 ��.R��� $ 
ItemBorder��#��z��#��z��#��z��._��  �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��$��28��$��28��8�� $ 6��$��0,0,12,0q�._��� $
��📦��$��14(�$��Center=�$��Center=��� $��1��$��Center=�$��13(����� �� $ RemoveButton��-   $��2��8�� $y�	Collapsed=�$��26��$��26��$ Remove product�� �� .���� ���#FF4C67$ 4��$��18��$��18��._��� $��×��$��14(�$
��Bold"�$��Center=�$��Center=�
������� P�  �@J�IsMouseOver$
 True��. ��� $ 
ItemBorder�� � ��	 �#32324A�� $ RemoveButton�� � y�$	 Visible=�P�  �@��
IsSelected$
 True��. ��� $ 
ItemBorder�� � ��	 �#383857�� $ RemoveButton�� � y�$	 Visible=��� $ 1�� ��  �� $ FindAncestor=�"  ��$! Items.Count�.a���  � ��$	 0q���  ��  �� $ PreviousData=� Z� .a���  � ��$	 0q��� " ��.���  � ��8	   ��  � ��$	 13(���  �@��TextAlignment$	 Center=���  �@��Padding$
	 0,16q���  � ��$	 Center=�.���� #��[ $ 12��$��10q�._�� .+��� -   " ��Name$" lblAuthWarning��$A��;Authentication required. Please sign in to access settings.��#��[ $��SemiBold"�$��13(�$��Center=�$	��Top=�$��0,12,0,0q�$y�	Collapsed=��� -   $ MainScrollViewer��$
��Auto=�$��24,16,24,32q�.���� .+��� $��Tenant Information��#��[ �� #��[ ._�� -   # �Name$# pnlTenantSettings�� �� $	��120 ��� $��* ��� $
��Auto �$ � RowDefinitions$ �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��
Tenant ID:��#��[ �� -   % ��Name$% txtTenantId��$��0��$��1��#��[ & �� 
IsReadOnly$
& True��$��0,0,0,12q��� $��1��$��0��$��Company Name:��#��[ �� -   $% txtTenantName��$��1��$��1��#��[ $
& True��$��0,0,0,12q��� $��2��$��0��$��Admin Email:��#��[ �� -   $% txtAdminEmail��$��2��$��1��#��[ $
& True���� -   $ btnRefreshTenantInfo��$��0��$��2��$��3��$��Refresh��#��[ $	��Top=�$��80���  �� $��* �.+��� $��0��$��Product Catalog��#��[ �� #��[ ._�� $ �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0�����#32324A$��0q�$ 6��$
 16,8q�$��0,0,0,8q�._�� .+��� $��Available Products��#��[ $��Medium"�$��13(�� $��1��.+��� -	   ' ��Name$' lstProducts��#��[ #��[ $	��150��$	��250���� $��2��$��0,1,0,0q�#��[ $ 0,16,0,0q�$��0,16,0,0q�._�� $ �� $
��Auto ��� $
��Auto �.+��� $��0��$��Add New Product��#��[ $��Medium"�$��0,0,0,8q�� $��1�� �� $��* ��� $
��Auto �.+��� -
   $% txtNewProduct��$��0��#��[ $��0,0,12,0q�.�� ( �� BasedOn) �� Triggers* j� TileMode+ j� Stretch, j� 
AlignmentX- !� Visual��%�   '��       7  �� " ��8(   ) P�  � ��$  ��. ���  � ��	 !� $
* None=�$
+ None=�$
, Left=�- �� $��Enter product name�����#808080$��12,6,0,0q�.���� -   $ btnAddProduct��$��1��$��Add Product��#��[ $	��120��