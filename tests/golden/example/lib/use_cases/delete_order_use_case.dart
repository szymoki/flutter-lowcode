// Generated from IR
import '../models/order.dart';
import '../presenters/pdelete_confirmation_message_presenter.dart';
import '../presenters/porder_deleted_message_presenter.dart';
import '../services/i_order.dart';

class DeleteOrderUseCase {
  final PDeleteConfirmationMessage _pDeleteConfirmationMessage;
  final POrderDeletedMessage _pOrderDeletedMessage;
  final IOrder _order;
  Order? _currentOrder;
  void Function(String result)? _onEnd;

  DeleteOrderUseCase({ required PDeleteConfirmationMessage pDeleteConfirmationMessage, required POrderDeletedMessage pOrderDeletedMessage, required IOrder order }) : _pDeleteConfirmationMessage = pDeleteConfirmationMessage,
      _pOrderDeletedMessage = pOrderDeletedMessage,
      _order = order;

  void setOrder(Order value) {
    _currentOrder = value;
  }

  void setOnEnd(void Function(String result) onEnd) {
    _onEnd = onEnd;
  }

  void execute() {}

  void handleDeleteOrder() {
    _hideViews();
    _pDeleteConfirmationMessage.showDeleteConfirmationMessage();
  }

  Future<void> handleDelete() async {
    await _order.deleteOrder(_currentOrder!);
    _hideViews();
    _pOrderDeletedMessage.showOrderDeletedMessage();
  }

  void handleOk() {
    _end('OK');
  }

  void handleCancel() {
    _end('CANCEL');
  }

  void _hideViews() {
    _pDeleteConfirmationMessage.hide();
    _pOrderDeletedMessage.hide();
  }

  void _end(String result) {
    _hideViews();
    final onEnd = _onEnd;
    _onEnd = null;
    onEnd?.call(result);
  }
}
