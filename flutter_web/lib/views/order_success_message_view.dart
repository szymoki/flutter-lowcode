// Generated from IR
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../presenters/porder_success_message_presenter.dart';
import '../controllers/order_success_message_controller.dart';

class OrderSuccessMessageView extends StatefulWidget {
  final COrderSuccessMessage controller;

  const OrderSuccessMessageView({super.key, required this.controller});

  @override
  State<OrderSuccessMessageView> createState() => _OrderSuccessMessageViewState();
}

class _OrderSuccessMessageViewState extends State<OrderSuccessMessageView> {
  final _numberController = TextEditingController();
  final _titleController = TextEditingController();
  final _datesController = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) context.read<POrderSuccessMessage>().show();
    });
  }

  @override
  void dispose() {
    _numberController.dispose();
    _titleController.dispose();
    _datesController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Consumer<POrderSuccessMessage>(
      builder: (context, presenter, child) {
        if (!presenter.isVisible) return const SizedBox.shrink();
        return Scaffold(
          appBar: AppBar(title: const Text('OrderSuccessMessage')),
          body: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _numberController, decoration: const InputDecoration(labelText: 'Number', border: OutlineInputBorder()), keyboardType: TextInputType.number,)),
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _titleController, decoration: const InputDecoration(labelText: 'Title', border: OutlineInputBorder()),)),
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _datesController, decoration: const InputDecoration(labelText: 'Dates', border: OutlineInputBorder()),)),
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
