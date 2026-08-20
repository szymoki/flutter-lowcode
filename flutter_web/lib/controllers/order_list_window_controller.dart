// Generated from IR
import '../use_cases/show_order_list_use_case.dart';

class COrderListWindow {
  final ShowOrderListUseCase _useCase;

  COrderListWindow({required ShowOrderListUseCase useCase}) : _useCase = useCase;

  void onClose() { _useCase.handleClose(); }
  void onOrderList() { _useCase.handleOrderList(); }
  void onAddOrder() { _useCase.handleAddOrder(); }
}
