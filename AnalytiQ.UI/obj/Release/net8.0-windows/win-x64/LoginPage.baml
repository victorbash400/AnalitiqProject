   M S B A M L   `   `   `  ���� F  BAnalytiQ.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:AnalytiQ.UIAnalytiQ.UI  '    !AnalytiQ.UI.AnalytiQ.UI.LoginPage   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     +localclr-namespace:AnalytiQ.UI       N� Title$  Login - AnalytiQ��$��Stretch=�$��Stretch=� N� 
Background �#121212 � 	Resources    ModernTextBox �� 
TargetType �� Property �� Value �� 
TargetType ��Name �� CornerRadius	 ��Name
 �� Triggers P� Property P� Value3  -System.Windows.Media.Effects.DropShadowEffect   ShadowDepth   Color   
BlurRadius   Opacity  ModernPasswordBox  
FieldLabel  PrimaryButton �� Padding  
LinkButton �� Triggers  SkipLoginLink  	InfoPopup��%�	  &	        &	 .    &	 \    &	 �    &	 �    &	 �    &	 r    �� " ��.���  � �� �#1E1E1E��  � �� �#E0E0E0��  � ��$ 14,12q���  � ��$ 14(���  � ��$ 1q���  � �� �#333333��  �@��
CaretBrush
 �������  � ��$
 True����  � ��$ 0����  �@�FocusVisualStyle Z� ��  � �� �� " ��.R��� $ border��#��z��#��z��#��z��$ 6��._�� .+��� $	 PART_ContentHost��#��z��$��Center=�
 P�  �@J�	IsFocused$
 True��. ���  � �� �#7C3AED��  �@J�Effect   $ 0Q�$ #7C3AED��$ 8Q�$	 0.4Q��� " G�.���  � �� �#1E1E1E��  � �� �#E0E0E0��  � ��$ 14,12q���  � ��$ 14(���  � ��$ 1q���  � �� �#333333��  �@G�
CaretBrush
 �������  � ��$
 True����  � ��$ 0����  �@�FocusVisualStyle Z� ��  � �� �� " G�.R��� $ border��#��z��#��z��#��z��$ 6��._�� .+��� $	 PART_ContentHost��#��z��$��Center=�
 P�  �@J�	IsFocused$
 True��. ���  � �� �#7C3AED��  �@J�Effect   $ 0Q�$ #7C3AED��$ 8Q�$	 0.4Q��� " ��.���  � �� �#ACACAC��  � ��$ 12(���  � ��$ SemiBold"���  � ��$ 0,0,0,6q��� " ��.���  � �� �#7C3AED��  � ��
 �������  � ��$ 14(���  � ��$ SemiBold"���  � ��$ 10,12q���  � ��$ 0q���  �@�Cursor$
 Hand����  � �� �� " ��.R��� $ border��#��z��$ 6��# z��._��� $��Center=�$��Center=�
 P�  �@J�IsMouseOver$
 True��. ���  � �� �#9361FB��  �@J�Effect   $ 0Q�$ #7C3AED��$ 10Q�$	 0.5Q�P�  � ��$
 True��. ���  � �� �#6B30CE�� " ��.���  � �� �#7C3AED��  � ��$ 14(���  �@�Cursor$
 Hand����  � ��$
 None{� P�  �@J�IsMouseOver$
 True��. ���  � ��$ 	Underline{���  � �� �#9361FB�� " ��.���  � �� �#666666��  � ��$ 12(���  �@�Cursor$
 Hand����  � ��$
 None{� P�  �@J�IsMouseOver$
 True��. ���  � ��$ 	Underline{���  � �� �#999999�� " ��.���  � �� �#222136��  � �� �#7C3AED��  � ��$ 1q���  �@��CornerRadius$ 8����  �@��Padding$ 15,12q���  �@J�Effect   $ 0Q�$ #000000��$ 15Q�$	 0.5Q�.��� ���#121212.+��� .+��� $
��1000��$	��650����L�  j� TileMode$
 Tile=� j� Viewport$ 	0,0,40,40� j� ViewportUnits$ Absolute=� L� Drawing I� .���  � Brush �#1A1A1A � Geometry ��  �� Rect$ 	0,0,40,40��  �#1E1E1E @�  @� RadiusX$ 1Q� @� RadiusY$ 1Q� @� Center$	 0,0&��� $ 16��$��20q� J� Effect   $ 0Q�$ Black��$ 25Q�$	 0.5Q�._��  � ColumnDefinitions �� $��* ��� $	��450 �.+��� $��0��$ 	16,0,0,16��$
}�True��._�� .+��� $��assets/why_not.jpg��$��UniformToFill=� -  'System.Windows.Media.Effects.BlurEffect     Radius$	 0.5Q��� ����   �� 
StartPoint$	  0,0&�! �� EndPoint$	! 1,1&�.��� " � Color$" 	#CC0D0D0D��# � Offset$	# 0.0Q�� $" 	#CC1A0A2E��$	# 1.0Q��� .+�F� ���#253341$��1��$ J� Opacity$	$ 0.3Q�% �� StrokeDashArray$	% 1,4R���� .���� & �� 
StartPoint$& 0,100&�' �� EndPoint$' 600,100&��� $& 0,200&�$' 600,200&��� $& 0,300&�$' 600,300&��� $& 0,400&�$' 600,400&��� $& 0,500&�$' 600,500&��� $& 100,0&�$' 100,600&��� $& 200,0&�$' 200,600&��� $& 300,0&�$' 300,600&��� $& 400,0&�$' 400,600&��� $	��200��$	��200��$	 100��$��-50,-50,0,0q�$
��Left=�$	��Top=���
� .��� $" 	#447C3AED��$# 0Q�� $" 	#007C3AED��$# 1Q��� $��Bottom=�$��
40,0,20,60q�.+��� $��Segoe UI)�$��ANALYTIQ��
�������$��36(�$
��Bold"�$��0,0,0,5q�   $ 0Q�$ #7C3AED��$ 10Q�$	 0.7Q��� $��HACKATHON EDITION�����#D0D0D0$��12(�$��SemiBold"�$��2,0,0,20q�$	$ 0.9Q�   $ 0Q�$ Black��$ 5Q�$	 0.5Q��� $-��'Advanced Business Intelligence Platform�����#E0E0E0$��14(�$
��Wrap=�$��0,0,0,15q�( �� 
LineHeight$( 20���� $����Transform your data into actionable insights with our powerful analytics dashboard. Visualize trends, identify opportunities, and make data-driven decisions with confidence.�����#CCCCCC$��14(�$
��Wrap=�$��0,0,0,20q�$( 24��$	��450��� .+��� ���	#22FFFFFF$ 15��$
 12,6q�$��0,0,8,8q�._��� $��Real-time Analytics�����#E0E0E0$��11(��� ���	#22FFFFFF$ 15��$
 12,6q�$��0,0,8,8q�._��� $��Interactive Dashboards�����#E0E0E0$��11(��� ���	#22FFFFFF$ 15��$
 12,6q�$��0,0,8,8q�._��� $��Predictive Insights�����#E0E0E0$��11(��� ���	#227C3AED$ 15��$
 12,6q�$��0,0,8,8q�._��� $��
AI-Powered�����#FFFFFF$��11(��� $��1�����#141414$ 	0,16,16,0��._�� .+��� $	��200��$	��200��$	 100��$��0,-100,-100,0q�$��Right=�$	��Top=���
� .��� $" 	#227C3AED��$# 0Q�� $" 	#001A1A1A��$# 1Q��� $��Center=�$��Center=�$��30,20q�$	��350��.+��� $��Sign In��$��30(�$
��Bold"����#FFFFFF$
��Left=�$��0,0,0,8q�   $ 0Q�$ #7C3AED��$ 15Q�$	 0.2Q��� $%��Access your analytics dashboard��$��14(����#B0B0B0$��0,0,0,40q�$
��Wrap=��� $��EMAIL ADDRESS��#��[ �� -   ) ��Name$) txtEmail��#��[  $��0,0,0,25q�$��1��4  .System.Windows.Automation.AutomationProperties*   Name$* Email Address���� $��PASSWORD��#��[ G� -   + G�Name$+ txtPassword��#��[ $��0,0,0,15q�$��2��$* Password���� -   , ��Name$, txtForgotPassword��$��Forgot your password?��#��[ $��Right=�$��0,0,0,30q��� -   $ 	infoPopup��#��[ $��0,0,0,20q�._��� - �� Orientation$- 
Horizontal=�.+��� $��ℹ️��$��16(�$��0,0,10,0q�$	��Top=��� ���#CCCCCC$��12(�$
��Wrap=�$( 18���� $3�-For hackathon testing, use these credentials:���� �� $3�-• bob@acmecorp.com / Pass123 = direct login���� �� $7�1• alice@newco.com / New456 = test company setup���� �� $4�.• ethan@techhub.io / Ethan456 = direct login��� -   . �Name$. loadingIndicator��/ � IsIndeterminate$
/ True��$��4�����#7C3AED$y�	Collapsed=�$��0,0,0,20q��� -   $ errorContainer�����#2E1619���#C62828$��1q�$ 6��$ 15,10q�$��0,0,0,25q�$y�	Collapsed=�._��� $- 
Horizontal=�.+��� $��⚠️��$��16(�$��0,0,10,0q��� -   $, txtErrorMessage�����#FF5252$��13(�$
��Wrap=��� -   0 ��Name$0 	btnSignIn��$��Sign In��$	��350��#��[ $��0,0,0,20q�$
{�True��1 �� 	IsDefault$
1 True��$��3��$* Sign In���� -	   $, btnSkipLogin��$��Skip Login (Dev Mode)��#��[ $��Center=�$��0,5,0,0q�$y�	Collapsed=�$* 
Skip Login���� $- 
Horizontal=�$��Center=�$��0,25,0,0q�.+��� $��Don't have access? ��$��13(����#A0A0A0�� -
   $, txtContactSupport��$��Contact support��#��[ 