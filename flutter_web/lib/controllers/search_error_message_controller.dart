// Generated from IR
import '../use_cases/find_order_use_case.dart';

class CSearchErrorMessage {
  final FindOrderUseCase _useCase;

  CSearchErrorMessage({required FindOrderUseCase useCase}) : _useCase = useCase;

  void onClose() { _useCase.handleClose(); }
  void onShowOrderList() { _useCase.handleShowOrderList(); }
}
