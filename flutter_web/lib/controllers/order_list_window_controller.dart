// Generated from IR
import '../use_cases/show_order_list_use_case.dart';
import '../use_cases/add_order_use_case.dart';

class COrderListWindow {
  final ShowOrderListUseCase _showOrderListUseCase;
  final AddOrderUseCase _addOrderUseCase;

  COrderListWindow({required ShowOrderListUseCase showOrderListUseCase, required AddOrderUseCase addOrderUseCase}) : _showOrderListUseCase = showOrderListUseCase, _addOrderUseCase = addOrderUseCase;

  void onClose() { _showOrderListUseCase.handleClose(); }
  void onOrderList() { _showOrderListUseCase.handleOrderList(); }
  void onAddOrder() { _addOrderUseCase.handleAddOrder(); }
}
