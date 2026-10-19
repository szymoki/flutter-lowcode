// Generated from IR
import '../presenters/pmain_menu_presenter.dart';


class StartUseCase {
  final PMainMenu _pMainMenu;



  StartUseCase({ required PMainMenu pMainMenu }) : _pMainMenu = pMainMenu;

  void execute() {}

  void handleApplication() {
    _hideViews();
    _pMainMenu.showMainMenu();
  }

  void _hideViews() {
    _pMainMenu.hide();
  }
}
