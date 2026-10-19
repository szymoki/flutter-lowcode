// Generated from IR
import 'package:flutter/foundation.dart';
import '../models/order_search.dart';


class POrderWindow extends ChangeNotifier {
  bool _isVisible = false;
  bool get isVisible => _isVisible;
  OrderSearch? _orderSearch;
  OrderSearch? get orderSearch => _orderSearch;

  void showOrderWindow(OrderSearch? orderSearch) {
        if (orderSearch != null) _orderSearch = orderSearch;
    notifyListeners();
  }


  void show() { _isVisible = true; notifyListeners(); }
  void hide() { _isVisible = false; notifyListeners(); }
  void clear() { _isVisible = false; notifyListeners(); }
}
