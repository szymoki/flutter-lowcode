// Generated from IR
import '../presenters/pdelete_confirmation_message_presenter.dart';
import '../presenters/porder_deleted_message_presenter.dart';
import '../services/i_order.dart';

class DeleteOrderUseCase {
  final PDeleteConfirmationMessage _pDeleteConfirmationMessage;
  final POrderDeletedMessage _pOrderDeletedMessage;
  final IOrder _order;

  DeleteOrderUseCase({ required PDeleteConfirmationMessage pDeleteConfirmationMessage, required POrderDeletedMessage pOrderDeletedMessage, required IOrder order }) : _pDeleteConfirmationMessage = pDeleteConfirmationMessage,
      _pOrderDeletedMessage = pOrderDeletedMessage,
      _order = order;

  void execute() {}
  void handleClose() {}
  void handleDelete() {}
  void handleOk() {}
  void handleCancel() {}
}
