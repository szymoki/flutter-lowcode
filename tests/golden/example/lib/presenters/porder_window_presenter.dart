// Generated from IR
import '../models/order.dart';
import 'view_presenter.dart';

class POrderWindow extends ViewPresenter {
  Order? _order;
  Order? get order => _order;

  void showOrderWindow({Order? order}) {
        if (order != null) _order = order;
    show();
  }
}
