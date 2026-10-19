// Generated from IR
import 'package:flutter/foundation.dart';

/// Widoczność widoku i kolejność wyświetlenia: na ekranie jest widoczny widok wyświetlony najpóźniej.
abstract class ViewPresenter extends ChangeNotifier {
  static int _showCounter = 0;

  bool _isVisible = false;
  int _shownAt = 0;

  bool get isVisible => _isVisible;
  int get shownAt => _shownAt;

  void show() {
    _isVisible = true;
    _shownAt = ++_showCounter;
    notifyListeners();
  }

  void hide() {
    if (!_isVisible) return;
    _isVisible = false;
    notifyListeners();
  }
}
