   M S B A M L   `   `   `  ���� F  BAnalytiQ.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:AnalytiQ.UIAnalytiQ.UI  0    *AnalytiQ.UI.AnalytiQ.UI.CompanySetupWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     +localclr-namespace:AnalytiQ.UI     -     � Title$#  Organization Setup - AnalytiQ�� � WindowStartupLocation$ CenterScreen=�$
��1000��$	��650�����#121212 � 
ResizeMode$ 	CanResize=� � WindowStyle$ SingleBorderWindow=� � 	Resources    ModernTextBox �� 
TargetType �� Property �� Value �� 
TargetType	 ��Name
 �� CornerRadius ��Name �� Triggers P� Property P� Value  ReadOnlyTextBox �� BasedOn  ModernComboBox  
FieldLabel �� Triggers  PrimaryButton �� Padding �� 
TargetName  
LinkButton ��Name  ModernCheckbox��%�  &	        &	 �    7   &	 �    &	 �    '�� #    &	 �    &	     &	 I    �� " ��.���  � �� �#1E1E1E��  � �� �#E0E0E0��  � ��$
 10,8q���  � ��$ 13(���  � ��$ 1q���  � �� �#333333��  �@��
CaretBrush
 �������  � ��$ 36����  � �� �� " ��.R��� $	 border��#��z��#��z��#��z��$
 4��._��� $ PART_ContentHost��#��z��$��Center=� P�  �@J�	IsFocused$
 True��. ���  � �� �#7C3AED�� " ��8   .���  � �� �#1A1A1A��  �@��
IsReadOnly$
 True���� " ��.���  � �� �#1E1E1E��  � �� �#E0E0E0��  � ��$
 10,8q���  � ��$ 13(���  � ��$ 1q���  � �� �#333333��  � ��$ 36���� " ��.���  � �� �#ACACAC��  � ��$ 11(���  � ��$ SemiBold"���  � ��$ 0,0,0,4q��� " ��.���  � ��$
 10,6q���  � �� �#E0E0E0 P�  �@��
IsSelected$
 True��. ���  � �� �#7C3AEDP�  �@J�IsMouseOver$
 True��. ���  � �� �#9361FB�� " ��.���  � �� �#7C3AED��  � ��
 �������  � ��$ 13(���  � ��$ SemiBold"���  � ��$
 16,8q���  � ��$ 0q���  � ��$ 36����  � �� �� " ��.R��� $	 border��#��z��$
 4��# z��._��� $��Center=�$��Center=� P�  �@J�IsMouseOver$
 True��. ��� $ border�� � �� �#9361FBP�  � ��$
 True��. ��� $ border�� � �� �#6B30CEP�  � {�$ False��. ��� $ border�� � �� �#4D4D4D��  � �� �#808080�� " ��.���  � �� �#ACACAC��  � ��$ 13(���  � ��
 ���� ��  � ��$ 0q���  � ��$	 8,8q���  � �� �� " ��.R��� $ LinkText��#��z��#��z�� P�  �@J�IsMouseOver$
 True��. ��� $ LinkText�� � �� �#9361FB�� $ LinkText�� � ��$ 	Underline{��� " ��.���  � �� �#E0E0E0��  � ��$ 13(�.��� ���#121212.+��� $
 8��$��15q�._��  � ColumnDefinitions �� $��* ��� $	��350 �.+��� $��0��$
 8,0,0,8��$
}�True��._�� .+��� $��/assets/why_not.jpg��$��UniformToFill=��� ����  �� 
StartPoint$	 0,0&� �� EndPoint$	 1,1&�.���  � Color$ 	#CC0D0D0D�� � Offset$	 0.0Q�� $ 	#CC2A0A4E��$	 1.0Q��� $��Center=�$��	30,0,20,0q�.+��� $��ANALYTIQ��
�������$��32(�$
��Bold"�$��0,0,0,5q��� $��Set Up Your Organization�����#E0E0E0$��16(�$��Medium"�$��0,0,0,8q��� $I��CRegister your company to access AI-powered analytics and dashboards�����#CCCCCC$��13(�$
��Wrap=�$��0,0,0,12q� �� 
LineHeight$ 18��$	��320��� $��0,10,0,0q�.+��� ���	#227C3AED$
 12��$
 10,5q�$��0,0,8,0q�._��� $��Real-time Analytics�����#E0E0E0$��11(��� ���	#447C3AED$
 12��$
 10,5q�$��0,0,8,0q�._��� $��
AI-Powered�����#FFFFFF$��11(��� $��1�����#141414$
 0,8,8,0��._�� $��25q�.+��� -   $	 errorContainer�����	#331E0A0A���#FF5252$��1q�$
 4��$ 8q�$��0,0,0,12q�$y�	Collapsed=�$	��Top=�._��� -   $ txtErrorMessage�����#FF5252$
��Wrap=�$��12(��� $��Center=�.+��� $��Organization Setup��$��20(�$
��Bold"����#FFFFFF$��0,0,0,4q��� $��Complete your profile��$��13(����#B0B0B0$��0,0,0,16q��� $��ORGANIZATION NAME��#��[ �� -    ��Name$ txtCompanyName��#��[  $��0,0,0,12q�$��1���� $��INDUSTRY��#��[ �� -    ��Name$ cboIndustry��#��[ $��0,0,0,12q�$��2��.@��� $��Finance���� $��Retail���� $��Manufacturing���� $��
Technology���� $��
Healthcare���� $��Other���� $��	TENANT ID��#��[ �� -   $ txtTenantId��#��[ $��0,0,0,12q�$��3���� $��ADMIN EMAIL��#��[ �� -   $ txtAdminEmail��#��[ $��0,0,0,16q�$��4���� ���#333333$��0,1,0,0q�$��0,0,0,16q��� -    ��Name$ 
chkConfirm��#��[ $��0,0,0,16q�$��5��.���� $=��7I confirm I am authorized to register this organization��$��12(�$
��Wrap=����#E0E0E0�  �� $��* ��� $
��Auto �.+��� -	    ��Name$ btnRegister��$��Register��$��0��#��[ $��0,0,8,0q�${�False�� �� 	IsDefault$
 True��$��6���� -
   $ btnSkipRegistration��$
��Skip��$��1��#��[ $��7��