// Generated from IR
import '../use_cases/show_order_list_use_case.dart';

class COrderListWindow {
  final ShowOrderListUseCase _showOrderListUseCase;

  COrderListWindow({required ShowOrderListUseCase showOrderListUseCase}) : _showOrderListUseCase = showOrderListUseCase;

  void onClose() { _showOrderListUseCase.handleClose(); }
  void onOrderList() { _showOrderListUseCase.handleOrderList(); }
}
