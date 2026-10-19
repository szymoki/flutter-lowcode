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
  final _titleController = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) context.read<POrderWindow>().show();
    });
  }

  @override
  void dispose() {
    _titleController.dispose();
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
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _titleController, decoration: const InputDecoration(labelText: 'Title', border: OutlineInputBorder()),)),
                ElevatedButton(onPressed: () => widget.controller.onClose(), child: const Text('close')),
                ElevatedButton(onPressed: () => widget.controller.onCancel(), child: const Text('cancel')),
              ],
            ),
          ),
        );
      },
    );
  }
}
