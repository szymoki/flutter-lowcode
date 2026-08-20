// Generated from IR
import '../use_cases/add_order_use_case.dart';

class CAddOrderForm {
  final AddOrderUseCase _useCase;

  CAddOrderForm({required AddOrderUseCase useCase}) : _useCase = useCase;

  void onAdd() { _useCase.handleAdd(); }
}
