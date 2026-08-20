// Generated from IR
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../presenters/porder_list_window_presenter.dart';
import '../controllers/order_list_window_controller.dart';

class OrderListWindowView extends StatefulWidget {
  final COrderListWindow controller;

  const OrderListWindowView({super.key, required this.controller});

  @override
  State<OrderListWindowView> createState() => _OrderListWindowViewState();
}

class _OrderListWindowViewState extends State<OrderListWindowView> {


  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) context.read<POrderListWindow>().show();
    });
  }

  @override
  void dispose() {

    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Consumer<POrderListWindow>(
      builder: (context, presenter, child) {
        if (!presenter.isVisible) return const SizedBox.shrink();
        return Scaffold(
          appBar: AppBar(title: const Text('OrderListWindow')),
          body: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                ElevatedButton(onPressed: () => widget.controller.onClose(), child: const Text('close')),
                ElevatedButton(onPressed: () => widget.controller.onOrderList(), child: const Text('orderList')),
                ElevatedButton(onPressed: () => widget.controller.onAddOrder(), child: const Text('addOrder')),
              ],
            ),
          ),
        );
      },
    );
  }
}
