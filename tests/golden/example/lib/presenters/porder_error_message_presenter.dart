// Generated from IR
import 'package:flutter/foundation.dart';
import '../models/order.dart';


class POrderErrorMessage extends ChangeNotifier {
  bool _isVisible = false;
  bool get isVisible => _isVisible;
  Order? _order;
  Order? get order => _order;

  void showOrderErrorMessage(Order? order) {
        if (order != null) _order = order;
    notifyListeners();
  }


  void show() { _isVisible = true; notifyListeners(); }
  void hide() { _isVisible = false; notifyListeners(); }
  void clear() { _isVisible = false; notifyListeners(); }
}
