// Generated from IR
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../presenters/porder_window_presenter.dart';
import '../controllers/order_window_controller.dart';

class OrderWindowView extends StatefulWidget {
  final COrderWindow controller;

  const OrderWindowView({super.key, required this.controller});

  @override
  State<OrderWindowView> createState() => _OrderWindowViewState();
}

class _OrderWindowViewState extends State<OrderWindowView> {


  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) context.read<POrderWindow>().show();
    });
  }

  @override
  void dispose() {

    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Consumer<POrderWindow>(
      builder: (context, presenter, child) {
        if (!presenter.isVisible) return const SizedBox.shrink();
        return Scaffold(
          appBar: AppBar(title: const Text('OrderWindow')),
          body: Padding(
            padding: const EdgeInsets.all(16.0),
            child: ListView(
              children: [
                ListTile(title: const Text('Number'), subtitle: Text('${presenter.order?.number ?? ''}')),
                ListTile(title: const Text('Title'), subtitle: Text('${presenter.order?.title ?? ''}')),
                ListTile(title: const Text('Dates'), subtitle: Text('${presenter.order?.dates ?? ''}')),
                ElevatedButton(onPressed: () => widget.controller.onClose(), child: const Text('close')),
              ],
            ),
          ),
        );
      },
    );
  }
}
