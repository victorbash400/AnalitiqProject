   M S B A M L   `   `   `  ����   AnalytiQ.UI'    !AnalytiQ.UI.AnalytiQ.UI.AdminPage   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008       N� Title$%  User Management - Admin Control�� N� 
Background �#1E1E2E � 	Resources    PrimaryBackColorValue  SecondaryBackColorValue  ContentBackColorValue  PrimaryTextColorValue  MutedTextColorValue  AccentColorValue  SecondaryAccentColorValue  CardBackgroundValue  BorderColorValue 	 WarningColorValue 
 SuccessColorValue  PrimaryBackColor �� Color  SecondaryBackColor  ContentBackColor  PrimaryTextColor  MutedTextColor  AccentColor  SecondaryAccentColor  CardBackground  BorderColor  WarningColor  SuccessColor  AccentGradient �� 
StartPoint �� EndPoint � Color � Offset3  -System.Windows.Media.Effects.DropShadowEffect  StandardShadow   ShadowDepth	   	Direction
   Opacity   
BlurRadius  
CardShadow  AccentShadow   Color  	CardStyle �� 
TargetType �� Property �� Value  ActionButtonStyle �� 
TargetType ��Name �� CornerRadius �� Triggers P� Property P� Value �� 
TargetName �� ScaleX �� ScaleY  RemoveButtonStyle �� BasedOn �� Orientation  RoleComboBoxStyle ��Name �� SelectionBoxItem �� SelectionBoxItemTemplate ��Name �Name  � AllowsTransparency! �� RelativeSource" �� Padding# ��Name$ ��Name-  'System.Windows.Input.KeyboardNavigation%   DirectionalNavigation  UserRowStyle& �� Triggers' :� RoutedEvent( �� TargetProperty	) �� To* g� Duration  HeaderTextStyle   UserDataTextStyle ! SearchBoxStyle+ J� Opacity, ]� 
Conditions- �� Property. �� Value " AvatarStyle��%k  &	        &	      &	 "     &	 3     &	 D     &	 U     &	 f     &	 w     &	 �     &		 �     &	
 �     &	 �     7   &	 �     7  &	 �     7  &	 �     7  &	 �     7  &	 �     7  &	 �     7  &	     7  &	     7  &	     7 	 &	     7 
 &	 )    7  7  &	 m    &	 �    &	 �    7  &	 �    7  &	 �    7  7  7  &	 Q    7  &	 �    7  7  7  7  7  7  7  7  7  7  &	     7  7  &	 �    7  &	  e    7  &	! �    7  7  7  7  7  7  7  7  &	" m    7  7  ��#1E1E2E����#282839����#2E2E42����#FFFFFF����#A6A6B6����#7E57C2����#9575CD����#252538����#3A3A54����#F06292����#66BB6A���� 8   �� 8   �� 8   �� 8   �� 8   �� 8   �� 8   �� 8   �� 8   �� 8   �� 8   �� $	 0,0&�$	 1,1&�.��� 8   $	 0.0Q�� 8  $	 1.0Q�  $ 2Q�$		 315Q�$	
 0.4Q�$ 10Q�  $ 1Q�$		 315Q�$	
 0.3Q�$ 8Q�  $ 0Q�$		 315Q�$	
 0.5Q�$ 10Q�8   �� " ��.���  � ��8   ��  �@��CornerRadius$ 14����  �@��Padding$ 24q���  �@J�Effect   $ Black��$ 14Q�$ 1Q�$

 0.25Q��� " ��.���  � ��$ 44����  � ��$
 24,0q���  � ��$	 8,0q���  � �� �� $	 0,0&�$	 1,0&�.��� 8   $	 0.0Q�� 8  $	 1.0Q���  � ��
 �������  � ��$ 0q���  � ��$ 14(���  � ��$ SemiBold"���  � ��$ Segoe UI)���  �@�Cursor$
 Hand����  � �� �� " ��.R��� $ ButtonBorder��#��z��$ 22��._��� $��Center=�$��Center=� P�  �@J�IsMouseOver$
 True��. ��� $ ButtonBorder�� �@J�Effect   $ 0Q�$ 12Q�$	
 0.5Q�8  �� $ ButtonBorder�� � z� �� $
 1.03Q�$
 1.03Q�P�  � ��$
 True��. ��� $ ButtonBorder�� � z� �� $
 0.97Q�$
 0.97Q��� " ��8   .���  � �� �� $	 0,0&�$	 1,0&�.��� $ #F06292��$	 0.0Q�� $ #EC407A��$	 1.0Q���  � ��$	 120����  �@�ToolTip$ Remove this user����  � �� �� " ��.R��� $ ButtonBorder��#��z��$ 22��._��� $ 
Horizontal=�$��Center=�$��Center=�.+��� $��🗑️��$��14(�$��Center=�$��0,0,8,0q��� $��Remove��$��Center=� P�  �@J�IsMouseOver$
 True��. ��� $ ButtonBorder�� �@J�Effect   $ 0Q�$ 12Q�$	
 0.5Q�$ #F06292���� $ ButtonBorder�� � z� �� $
 1.03Q�$
 1.03Q�P�  � ��$
 True��. ��� $ ButtonBorder�� � z� �� $
 0.97Q�$
 0.97Q��� " ��.���  � ��$ 40����  � ��$	 170����  � ��$	 8,0q���  � ��$ 14(���  � ��$ Segoe UI)���  � ��$ Center=���  � ��$ 1q���  � ��8   ��  � ��8  ��  � ��8  ��  � ��$
 12,0q���  � �� �� " ��.R�� .+��� $ border��$ 20��#��z��#��z��#��z��._�� .+��� $ contentPresenter��#��z #��z #��z��#��z��#��z���� $ arrow��$	��▼��$��10(�$��Right=�$��Center=�$��0,0,12,0q�8�� � $ 
PART_Popup��$
  True��$��Bottom=�$
��Fade=����� *IsDropDownOpen+! �� *TemplatedParent+.���� $ dropDownBorder��$ 10��8�� 8�� $��1q�$" 4q�$��0,6,0,0q�._��� $# DropDownScrollViewer��$	��200��.���� $$ ItemsPresenter��$% 	Contained=� P�  �@J�IsMouseOver$
 True��. ��� $ border��8   � ��P�  �@��IsDropDownOpen$
 True��. ��� $ border��8   � ���� $ arrow��8   � ���� $ border�� �@J�Effect   $ 0Q�$ 8Q�$	
 0.3Q�8 	 �� " ��.���  �@��Padding$ 18,16q���  �@��CornerRadius$ 12����  � ��$	 0,8q���  � ��
 ���� ��  � ��$ 1q���  � ��
 ���� & :� $' 
MouseEnter��.[��� .���� .���� $( Background.Color�8)   $* 0:0:0.2D��� $( BorderBrush.Color�8)  $* 0:0:0.2D�:� $' 
MouseLeave��.[��� .���� .���� $( Background.Color�) Transparent$* 0:0:0.2D��� $( BorderBrush.Color�) Transparent$* 0:0:0.2D��� " ��.���  � ��$ SemiBold"���  � ��8   ��  � ��$ 14(���  � ��$ 14,0,0,0q���  �@J�Opacity$
 0.85Q��� " ��.���  � ��8   ��  � ��$ 14(���  � ��$ Center=��� " ��.���  � ��$ 44����  � ��$	 300����  � ��$ 	46,0,16,0q���  � ��$ Center=���  � ��$ 1q���  � ��8   ��  � ��8  ��  � ��8  ��  � ��$ 14(���  � �� �� " ��.R��� $ border��#��z��#��z��#��z��$ 22��._�� .+��� $ 
searchIcon��$
��🔍��8�� $��Center=�$��18,0,0,0q�$��16(�$	+ 0.8Q��� $# PART_ContentHost��#��z���� $ WatermarkText��$��Search users...��8�� $��Center=�$��46,0,0,0q�$y�	Collapsed=� P�  �@J�IsKeyboardFocused$
 True��. ��� $ border��8   � ���� $ 
searchIcon�� �@J�Opacity$ 1Q��� $ border�� �@J�Effect   $ 0Q�$ 12Q�$	
 0.3Q�8  ]� , �� - � ��$.  ���� - �@J�IsKeyboardFocused$. False��./��� $ WatermarkText�� � y�$ Visible=�P�  �@J�IsMouseOver$
 True��. ��� $ border��8   � ���� $ 
searchIcon�� �@J�Opacity$ 1Q��� " ��.���  � ��$ 40����  � ��$ 40����  �@��CornerRadius$ 20����  � ��$ 0,0,14,0q���  � ��8   ��  � ��$ 2q���  � ��8  ��  �@J�Effect   $ 0Q�$ 8Q�$	
 0.2Q�$ Black��.��� $��32q�/ � RowDefinitions/ �� $
��Auto ��� $��* ��� $
��Auto �.+�� $��0��$��0,0,0,28q�0 � ColumnDefinitions0 �� $��* ��� $
��Auto �.+��� $��0��.+��� $��Tenant User Management��$��28(�$��SemiBold"�#��[ 1 J� Effect1   $ 1Q�$ 5Q�$	
 0.2Q�$ Black���� $ 
Horizontal=�$��0,10,0,0q�.+��� #��[ $ 8��$
" 10,4q�._��� $��Admin��
�������$��13(�$��SemiBold"��� -   $ txtTenantInfo��$��Tenant: Loading...��#��[ $��15(�$��14,0,0,0q��� $��1��$ 
Horizontal=�.+��� -   2 ��Name$2 	txtSearch��#��[! $��0,0,16,0q��� -   3 ��Name$3 btnInviteEmployee��#��[ .���� $ 
Horizontal=�.+��� $	��➕��$��16(�$��Center=�$��0,0,10,0q��� $��Invite Employee��$��Center=��� $��1��#��[ ._�� / �� $
��Auto ��� $��* �.+��� $��0��$��0,0,0,16q�#��[ $ 12��$��1q�#��[ ._�� $��16,14q�0 �� $��2* ��� $��3* ��� $
��1.5* ��� $��1* �.+��� $��0��$
��Name��#��[ �� $��1��$��Email��#��[ �� $��2��$
��Role��#��[ �� $��3��$��Actions��#��[ �� $��1��$
��Auto=�$��0,0,8,0q�.���� -   4 ��Name$4 
UsersPanel���� $��2��#��[ $��0,20,0,0q�._�� .+��� $
��Left=�$��Center=�.+��� $ 
Horizontal=�.+��� #��[ $��36��$��36��$ 18��$��Center=�._��� $
��💾��$��16(�$��Center=�$��Center=��� $%��Changes are saved automatically��#��[ $��14(�$��Center=�$��12,0,0,0q��� $ 
Horizontal=�$��Right=�.+��� -   $3 btnSaveChanges��#��[ #��[ .���� $ 
Horizontal=�.+��� $	��✅��$��16(�$��Center=�$��0,0,10,0q��� $��Save Changes��$��Center=�