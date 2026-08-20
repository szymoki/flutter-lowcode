// Generated from IR
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../presenters/porder_deleted_message_presenter.dart';
import '../controllers/order_deleted_message_controller.dart';

class OrderDeletedMessageView extends StatefulWidget {
  final COrderDeletedMessage controller;

  const OrderDeletedMessageView({super.key, required this.controller});

  @override
  State<OrderDeletedMessageView> createState() => _OrderDeletedMessageViewState();
}

class _OrderDeletedMessageViewState extends State<OrderDeletedMessageView> {


  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) context.read<POrderDeletedMessage>().show();
    });
  }

  @override
  void dispose() {

    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Consumer<POrderDeletedMessage>(
      builder: (context, presenter, child) {
        if (!presenter.isVisible) return const SizedBox.shrink();
        return Scaffold(
          appBar: AppBar(title: const Text('OrderDeletedMessage')),
          body: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                ElevatedButton(onPressed: () => widget.controller.onOk(), child: const Text('ok')),
                ElevatedButton(onPressed: () => widget.controller.onCancel(), child: const Text('cancel')),
              ],
            ),
          ),
        );
      },
    );
  }
}
