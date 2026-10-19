// Generated from IR
import 'package:flutter/foundation.dart';
import '../models/order_list.dart';


class POrderListWindow extends ChangeNotifier {
  bool _isVisible = false;
  bool get isVisible => _isVisible;
  OrderList? _orderList;
  OrderList? get orderList => _orderList;

  void showOrderListWindow(OrderList? orderList) {
        if (orderList != null) _orderList = orderList;
    notifyListeners();
  }


  void show() { _isVisible = true; notifyListeners(); }
  void hide() { _isVisible = false; notifyListeners(); }
  void clear() { _isVisible = false; notifyListeners(); }
}
