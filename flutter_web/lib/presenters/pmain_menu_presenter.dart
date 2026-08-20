// Generated from IR
import 'package:flutter/foundation.dart';


class PMainMenu extends ChangeNotifier {
  bool _isVisible = false;
  bool get isVisible => _isVisible;


  void showMainMenu() {
    notifyListeners();
  }


  void show() { _isVisible = true; notifyListeners(); }
  void hide() { _isVisible = false; notifyListeners(); }
  void clear() { _isVisible = false; notifyListeners(); }
}
