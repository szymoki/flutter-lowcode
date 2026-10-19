// Generated from IR
import '../models/order_list.dart';
import '../presenters/porder_list_window_presenter.dart';
import '../services/i_order_list.dart';

class ShowOrderListUseCase {
  final POrderListWindow _pOrderListWindow;
  final IOrderList _orderList;
  OrderList? _currentOrderList;

  ShowOrderListUseCase({ required POrderListWindow pOrderListWindow, required IOrderList orderList }) : _pOrderListWindow = pOrderListWindow,
      _orderList = orderList;

  void execute() {}

  Future<void> handleShowOrderList() async {
    _currentOrderList = await _orderList.readOrderList();
    _hideViews();
    _pOrderListWindow.showOrderListWindow(orderList: _currentOrderList);
  }

  void handleClose() {
    _end('OK');
  }

  Future<void> handleOrderList() async {
    _currentOrderList = await _orderList.readOrderList();
    _hideViews();
    _pOrderListWindow.showOrderListWindow(orderList: _currentOrderList);
  }

  void _hideViews() {
    _pOrderListWindow.hide();
  }

  void _end(String result) {
    _hideViews();
  }
}
