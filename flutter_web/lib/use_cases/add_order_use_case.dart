// Generated from IR
import '../models/order.dart';
import '../models/order_check_result.dart';
import '../presenters/padd_order_form_presenter.dart';
import '../presenters/porder_success_message_presenter.dart';
import '../presenters/porder_error_message_presenter.dart';
import '../services/i_order.dart';

class AddOrderUseCase {
  final PAddOrderForm _pAddOrderForm;
  final POrderSuccessMessage _pOrderSuccessMessage;
  final POrderErrorMessage _pOrderErrorMessage;
  final IOrder _order;
  Order? _currentOrder;
  OrderCheckResult? _orderCheckResult;

  AddOrderUseCase({ required PAddOrderForm pAddOrderForm, required POrderSuccessMessage pOrderSuccessMessage, required POrderErrorMessage pOrderErrorMessage, required IOrder order }) : _pAddOrderForm = pAddOrderForm,
      _pOrderSuccessMessage = pOrderSuccessMessage,
      _pOrderErrorMessage = pOrderErrorMessage,
      _order = order;

  void execute() {}

  void handleAddOrder() {
    _hideViews();
    _pAddOrderForm.showAddOrderForm();
  }

  Future<void> handleAdd(Order order) async {
    _currentOrder = order;
    _orderCheckResult = await _order.checkOrder(_currentOrder!);
    if (_orderCheckResult == OrderCheckResult.ok) {
      await _order.updateOrder(_currentOrder!);
      _hideViews();
      _pOrderSuccessMessage.showOrderSuccessMessage();
    } else if (_orderCheckResult == OrderCheckResult.notOk) {
      _hideViews();
      _pOrderErrorMessage.showOrderErrorMessage();
    }
  }

  void handleOk() {
    _end('OK');
  }

  void handleCancel() {
    _end('CANCEL');
  }

  void handleClose() {
    _end('notOK');
  }

  void handleCorrect() {
    _hideViews();
    _pAddOrderForm.showAddOrderForm();
  }

  void _hideViews() {
    _pAddOrderForm.hide();
    _pOrderSuccessMessage.hide();
    _pOrderErrorMessage.hide();
  }

  void _end(String result) {
    _hideViews();
  }
}
