   M S B A M L   `   `   `  ����  WPF_Client_1    WPF_Client_1.MainWindow   5   	   W SPresentationCore, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Chat Client��5   	   $	��450��6   $	��800��6*   .��5      � .+�5   
   � -   5         �Name$ 	LoginPage��5      .+�5      �� $��Center=�6   $��Center=�66   .+�5      �� -   5         ��Name$ UsernameInput��5      $	��200��61   G� 5	      -   5         G�Name$ PasswordInput��5	      $	��200��65   �� 5
      -   5        $��Login��5
      6)   �� 5      -   5        $��Register��5      6,   �� 5      -   5         ��Name$ LoginMessage��5      
���  ��$y�	Collapsed=�6C   5      5      � 5   
   -   5        $ ChatPage��5      $y�	Collapsed=�6!    � RowDefinitions 5      �� 5      $��* �6    �� 5      $
��Auto �6    .+�5      �� $��0��6   .��5      �� -   5         ��Name$ MessagesPanel��5      5      �� 5      $��1��6    �� Orientation$ 
Horizontal=�6&   .+�5      �� -	   5        $ MessageInput��5      $	��400��60   �� 5      -
   5        $
��Send��5      6(   �� 5      -   5        $��
Send Image��5      6.   �� 5      -   5        $��
Send Audio��5      6.   5      5      5      5      