// Generated from IR
import '../models/order_list.dart';
import 'view_presenter.dart';

class POrderListWindow extends ViewPresenter {
  OrderList? _orderList;
  OrderList? get orderList => _orderList;

  void showOrderListWindow({OrderList? orderList}) {
        if (orderList != null) _orderList = orderList;
    show();
  }
}
