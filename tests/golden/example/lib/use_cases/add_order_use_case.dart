// Generated from IR
import '../presenters/padd_order_form_presenter.dart';
import '../presenters/porder_success_message_presenter.dart';
import '../presenters/porder_error_message_presenter.dart';
import '../services/i_order.dart';

class AddOrderUseCase {
  final PAddOrderForm _pAddOrderForm;
  final POrderSuccessMessage _pOrderSuccessMessage;
  final POrderErrorMessage _pOrderErrorMessage;
  final IOrder _order;

  AddOrderUseCase({ required PAddOrderForm pAddOrderForm, required POrderSuccessMessage pOrderSuccessMessage, required POrderErrorMessage pOrderErrorMessage, required IOrder order }) : _pAddOrderForm = pAddOrderForm,
      _pOrderSuccessMessage = pOrderSuccessMessage,
      _pOrderErrorMessage = pOrderErrorMessage,
      _order = order;

  void execute() {}
  void handleAddOrder() {}
  void handleAdd() {}
  void handleOk() {}
  void handleCancel() {}
  void handleClose() {}
  void handleCorrect() {}
}
